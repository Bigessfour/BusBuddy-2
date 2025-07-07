using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBuddy.Core.Models;

/// <summary>
/// Represents a daily route record
/// Based on Routes Table from BusBuddy Tables schema
/// </summary>
[Table("Routes")]
public class Route
{
    [Key]
    public int RouteId { get; set; }

    [Required]
    [Display(Name = "Date")]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Route Name")]
    public string RouteName { get; set; } = string.Empty; // Truck Plaza, East Route, West Route, SPED

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    // AM Route Information
    [ForeignKey("AMVehicle")]
    [Display(Name = "AM Vehicle")]
    public int? AMVehicleId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "AM Begin Miles")]
    public decimal? AMBeginMiles { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "AM End Miles")]
    public decimal? AMEndMiles { get; set; }

    [Range(0, 100)]
    [Display(Name = "AM Riders")]
    public int? AMRiders { get; set; }

    [ForeignKey("AMDriver")]
    [Display(Name = "AM Driver")]
    public int? AMDriverId { get; set; }

    // PM Route Information
    [ForeignKey("PMVehicle")]
    [Display(Name = "PM Vehicle")]
    public int? PMVehicleId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "PM Begin Miles")]
    public decimal? PMBeginMiles { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "PM End Miles")]
    public decimal? PMEndMiles { get; set; }

    [Range(0, 100)]
    [Display(Name = "PM Riders")]
    public int? PMRiders { get; set; }

    [ForeignKey("PMDriver")]
    [Display(Name = "PM Driver")]
    public int? PMDriverId { get; set; }

    // Additional properties for route analysis and optimization
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Distance (Miles)")]
    public decimal? Distance { get; set; }

    [Display(Name = "Estimated Duration (Minutes)")]
    public int? EstimatedDuration { get; set; }

    [Display(Name = "Student Count")]
    public int? StudentCount { get; set; }

    [Display(Name = "Stop Count")]
    public int? StopCount { get; set; }

    [Display(Name = "AM Begin Time")]
    public TimeSpan? AMBeginTime { get; set; }

    [Display(Name = "PM Begin Time")]
    public TimeSpan? PMBeginTime { get; set; }

    [StringLength(100)]
    [Display(Name = "Driver Name")]
    public string? DriverName { get; set; }

    [StringLength(20)]
    [Display(Name = "Bus Number")]
    public string? BusNumber { get; set; }

    // Navigation properties
    public virtual Bus? AMVehicle { get; set; }
    public virtual Bus? PMVehicle { get; set; }
    public virtual Driver? AMDriver { get; set; }
    public virtual Driver? PMDriver { get; set; }
}
