using BusBuddy.Core.Data.Interfaces;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Core.Data.Repositories;

/// <summary>
/// Student-specific repository implementation
/// Extends generic repository with student-specific operations
/// </summary>
public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(BusBuddyDbContext context, IUserContextService userContextService) : base(context, userContextService)
    {
    }

    #region Async Student-Specific Operations

    public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
    {
        return await Query()
            .Where(s => s.Active)
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade)
    {
        return await FindAsync(s => s.Grade == grade && s.Active);
    }

    public async Task<IEnumerable<Student>> GetStudentsBySchoolAsync(string school)
    {
        return await FindAsync(s => s.School == school && s.Active);
    }

    public async Task<Student?> GetStudentByStudentNumberAsync(string studentNumber)
    {
        return await FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
    }

    public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetActiveStudentsAsync();

        return await Query()
            .Where(s => s.Active &&
                       (s.StudentName != null && s.StudentName.Contains(searchTerm) ||
                        s.StudentNumber != null && s.StudentNumber.Contains(searchTerm) ||
                        s.School != null && s.School.Contains(searchTerm)))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsByRouteAsync(int? routeId)
    {
        string routeIdStr = routeId?.ToString() ?? string.Empty;
        return await Query()
            .Where(s => s.Active && (s.AMRoute == routeIdStr || s.PMRoute == routeIdStr))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsWithoutRouteAsync()
    {
        return await Query()
            .Where(s => s.Active && string.IsNullOrEmpty(s.AMRoute) && string.IsNullOrEmpty(s.PMRoute))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<Student?> GetStudentByNameAsync(string name)
    {
        return await FirstOrDefaultAsync(s => s.StudentName == name);
    }

    public async Task<IEnumerable<Student>> SearchStudentsByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetActiveStudentsAsync();

        return await Query()
            .Where(s => s.Active && s.StudentName.Contains(searchTerm))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsWithSpecialNeedsAsync()
    {
        return await Query()
            .Where(s => s.Active && s.SpecialNeeds)
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsWithMedicalConditionsAsync()
    {
        return await Query()
            .Where(s => s.Active && !string.IsNullOrEmpty(s.MedicalNotes))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsRequiringSpecialTransportationAsync()
    {
        // No RequiresSpecialTransportation property; use SpecialNeeds or SpecialAccommodations as proxy
        return await Query()
            .Where(s => s.Active && (s.SpecialNeeds || !string.IsNullOrEmpty(s.SpecialAccommodations)))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsWithEmergencyContactsAsync()
    {
        return await Query()
            .Where(s => s.Active && !string.IsNullOrEmpty(s.EmergencyPhone))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsWithoutEmergencyContactsAsync()
    {
        return await Query()
            .Where(s => s.Active && string.IsNullOrEmpty(s.EmergencyPhone))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsByTransportationTypeAsync(string transportationType)
    {
        return await Query()
            .Where(s => s.Active && s.TransportationNotes == transportationType)
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsEligibleForRouteAsync(int routeId)
    {
        string routeIdStr = routeId.ToString();
        return await Query()
            .Where(s => s.Active && (string.IsNullOrEmpty(s.AMRoute) && string.IsNullOrEmpty(s.PMRoute) || (s.AMRoute != routeIdStr && s.PMRoute != routeIdStr)))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<int> GetStudentCountByRouteAsync(int routeId)
    {
        string routeIdStr = routeId.ToString();
        return await CountAsync(s => s.Active && (s.AMRoute == routeIdStr || s.PMRoute == routeIdStr));
    }

    public async Task<Dictionary<string, int>> GetStudentCountByRouteAsync()
    {
        return await Query()
            .Where(s => s.Active && (!string.IsNullOrEmpty(s.AMRoute) || !string.IsNullOrEmpty(s.PMRoute)))
            .GroupBy(s => s.AMRoute ?? s.PMRoute ?? "Unknown")
            .ToDictionaryAsync(g => g.Key ?? "Unknown", g => g.Count());
    }

    public async Task<int> GetTotalStudentCountAsync()
    {
        return await CountAsync();
    }

    public async Task<int> GetActiveStudentCountAsync()
    {
        return await CountAsync(s => s.Active);
    }

    public async Task<Dictionary<string, int>> GetStudentCountByGradeAsync()
    {
        return await Query()
            .Where(s => s.Active && s.Grade != null)
            .GroupBy(s => s.Grade!)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetStudentCountByTransportationTypeAsync()
    {
        return await Query()
            .Where(s => s.Active)
            .GroupBy(s => s.TransportationNotes)
            .ToDictionaryAsync(g => g.Key ?? "Unknown", g => g.Count());
    }

    public async Task<IEnumerable<Student>> GetStudentsByAgeRangeAsync(int minAge, int maxAge)
    {
        var today = DateTime.Today;
        var minBirthDate = today.AddYears(-maxAge - 1);
        var maxBirthDate = today.AddYears(-minAge);

        return await Query()
            .Where(s => s.Active &&
                       s.DateOfBirth.HasValue &&
                       s.DateOfBirth.Value >= minBirthDate &&
                       s.DateOfBirth.Value <= maxBirthDate)
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsByParentEmailAsync(string email)
    {
        // No ParentEmail/GuardianEmail; use ParentGuardian as proxy (search by name)
        return await Query()
            .Where(s => s.Active && (s.ParentGuardian != null && s.ParentGuardian.Contains(email)))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsByParentPhoneAsync(string phone)
    {
        // No ParentPhone/GuardianPhone; use HomePhone, EmergencyPhone, AlternativePhone
        return await Query()
            .Where(s => s.Active && (s.HomePhone == phone || s.EmergencyPhone == phone || s.AlternativePhone == phone))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsWithIncompleteContactInfoAsync()
    {
        // No ParentPhone/ParentEmail; use ParentGuardian and HomePhone as required fields
        return await Query()
            .Where(s => s.Active && (string.IsNullOrEmpty(s.ParentGuardian) || string.IsNullOrEmpty(s.HomePhone)))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsWithActivityPermissionsAsync()
    {
        return await Query()
            .Where(s => s.Active && (s.PhotoPermission || s.FieldTripPermission))
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsWithoutActivityPermissionsAsync()
    {
        return await Query()
            .Where(s => s.Active && !s.PhotoPermission && !s.FieldTripPermission)
            .OrderBy(s => s.StudentName)
            .ToListAsync();
    }

    #endregion

    #region Synchronous Student-Specific Operations

    public IEnumerable<Student> GetActiveStudents()
    {
        return Query()
            .Where(s => s.Active)
            .OrderBy(s => s.StudentName)
            .ToList();
    }

    public IEnumerable<Student> GetStudentsByGrade(string grade)
    {
        return Find(s => s.Grade == grade && s.Active);
    }

    public IEnumerable<Student> GetStudentsBySchool(string school)
    {
        return Find(s => s.School == school && s.Active);
    }

    public Student? GetStudentByStudentNumber(string studentNumber)
    {
        return FirstOrDefault(s => s.StudentNumber == studentNumber);
    }

    public IEnumerable<Student> SearchStudents(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return GetActiveStudents();

        return Query()
            .Where(s => s.Active &&
                       (s.StudentName.Contains(searchTerm) ||
                        (s.StudentNumber != null && s.StudentNumber.Contains(searchTerm)) ||
                        (s.School != null && s.School.Contains(searchTerm))))
            .OrderBy(s => s.StudentName)
            .ToList();
    }

    public IEnumerable<Student> GetStudentsByRoute(int? routeId)
    {
        string routeIdStr = routeId?.ToString() ?? string.Empty;
        return Query()
            .Where(s => s.Active && (s.AMRoute == routeIdStr || s.PMRoute == routeIdStr))
            .OrderBy(s => s.StudentName)
            .ToList();
    }

    public IEnumerable<Student> GetStudentsWithoutRoute()
    {
        return Query()
            .Where(s => s.Active && string.IsNullOrEmpty(s.AMRoute) && string.IsNullOrEmpty(s.PMRoute))
            .OrderBy(s => s.StudentName)
            .ToList();
    }

    public IEnumerable<Student> GetStudentsWithSpecialNeeds()
    {
        return Query()
            .Where(s => s.Active && s.SpecialNeeds)
            .OrderBy(s => s.StudentName)
            .ToList();
    }

    public IEnumerable<Student> SearchStudentsByName(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return GetActiveStudents();

        return Query()
            .Where(s => s.Active && s.StudentName.Contains(searchTerm))
            .OrderBy(s => s.StudentName)
            .ToList();
    }

    public int GetStudentCountByRoute(int routeId)
    {
        string routeIdStr = routeId.ToString();
        return Count(s => s.Active && (s.AMRoute == routeIdStr || s.PMRoute == routeIdStr));
    }

    #endregion
}
