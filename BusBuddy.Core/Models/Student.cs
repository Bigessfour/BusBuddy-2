using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBuddy.Core.Models;

/// <summary>
/// Represents a student in the school bus system
/// Based on Students Table from BusBuddy Tables schema
/// Enhanced for Syncfusion data binding with INotifyPropertyChanged support
/// </summary>
[Table("Students")]
public class Student : INotifyPropertyChanged
{
    [Key]
    public int StudentId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Student Name")]
    public string StudentName { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Student Number")]
    public string? StudentNumber { get; set; }

    [StringLength(20)]
    [Display(Name = "Grade")]
    public string? Grade { get; set; }

    [StringLength(100)]
    [Display(Name = "School")]
    public string? School { get; set; }

    [StringLength(200)]
    [Display(Name = "Home Address")]
    public string? HomeAddress { get; set; }

    [StringLength(50)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [StringLength(2)]
    [Display(Name = "State")]
    public string? State { get; set; }

    [StringLength(10)]
    [Display(Name = "Zip Code")]
    public string? Zip { get; set; }

    [StringLength(20)]
    [Display(Name = "Home Phone")]
    public string? HomePhone { get; set; }

    [StringLength(100)]
    [Display(Name = "Parent/Guardian")]
    public string? ParentGuardian { get; set; }

    [StringLength(20)]
    [Display(Name = "Emergency Phone")]
    public string? EmergencyPhone { get; set; }

    [StringLength(1000)]
    [Display(Name = "Medical Notes")]
    public string? MedicalNotes { get; set; }

    [StringLength(1000)]
    [Display(Name = "Transportation Notes")]
    public string? TransportationNotes { get; set; }

    [Display(Name = "Active")]
    public bool Active { get; set; } = true;

    [Display(Name = "Enrollment Date")]
    public DateTime? EnrollmentDate { get; set; }

    // Bus assignment information
    [StringLength(50)]
    [Display(Name = "AM Route")]
    public string? AMRoute { get; set; }

    [StringLength(50)]
    [Display(Name = "PM Route")]
    public string? PMRoute { get; set; }

    [StringLength(50)]
    [Display(Name = "Bus Stop")]
    public string? BusStop { get; set; }

    // Extended properties for maximum flexibility
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [StringLength(200)]
    [Display(Name = "Pickup Address")]
    public string? PickupAddress { get; set; }

    [StringLength(200)]
    [Display(Name = "Dropoff Address")]
    public string? DropoffAddress { get; set; }

    [Display(Name = "Special Needs")]
    public bool SpecialNeeds { get; set; } = false;

    [StringLength(1000)]
    [Display(Name = "Special Accommodations")]
    public string? SpecialAccommodations { get; set; }

    [StringLength(200)]
    [Display(Name = "Allergies")]
    public string? Allergies { get; set; }

    [StringLength(200)]
    [Display(Name = "Medications")]
    public string? Medications { get; set; }

    [StringLength(100)]
    [Display(Name = "Doctor Name")]
    public string? DoctorName { get; set; }

    [StringLength(20)]
    [Display(Name = "Doctor Phone")]
    public string? DoctorPhone { get; set; }

    [StringLength(100)]
    [Display(Name = "Alternative Contact")]
    public string? AlternativeContact { get; set; }

    [StringLength(20)]
    [Display(Name = "Alternative Phone")]
    public string? AlternativePhone { get; set; }

    [Display(Name = "Photo Permission")]
    public bool PhotoPermission { get; set; } = true;

    [Display(Name = "Field Trip Permission")]
    public bool FieldTripPermission { get; set; } = true;

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated Date")]
    public DateTime? UpdatedDate { get; set; }

    [StringLength(100)]
    [Display(Name = "Created By")]
    public string? CreatedBy { get; set; }

    [StringLength(100)]
    [Display(Name = "Updated By")]
    public string? UpdatedBy { get; set; }

    // Computed properties
    [NotMapped]
    [Display(Name = "Age")]
    public int? Age
    {
        get => DateOfBirth.HasValue ? DateTime.Now.Year - DateOfBirth.Value.Year : null;
    }

    [NotMapped]
    [Display(Name = "Full Name")]
    public string FullName => StudentName;

    [NotMapped]
    [Display(Name = "Contact Info")]
    public string ContactInfo
    {
        get
        {
            var parts = new[] { ParentGuardian, HomePhone }.Where(p => !string.IsNullOrEmpty(p));
            return string.Join(" - ", parts);
        }
    }

    [NotMapped]
    [Display(Name = "Has Special Needs")]
    public bool HasSpecialNeeds => SpecialNeeds || !string.IsNullOrEmpty(MedicalNotes) || !string.IsNullOrEmpty(SpecialAccommodations);

    [NotMapped]
    [Display(Name = "Grade Level")]
    public string GradeLevel => Grade ?? "Unknown";

    // INotifyPropertyChanged implementation for Syncfusion data binding
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
