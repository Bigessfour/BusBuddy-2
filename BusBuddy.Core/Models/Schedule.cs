using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBuddy.Core.Models;

/// <summary>
/// Represents a scheduled bus operation
/// Extended model for enhanced scheduling functionality with Syncfusion components
/// </summary>
[Table("Schedules")]
public class Schedule
{
    [Key]
    public int ScheduleId { get; set; }

    [Required]
    [ForeignKey("Bus")]
    [Display(Name = "Bus")]
    public int BusId { get; set; }

    [Required]
    [ForeignKey("Route")]
    [Display(Name = "Route")]
    public int RouteId { get; set; }

    [Required]
    [ForeignKey("Driver")]
    [Display(Name = "Driver")]
    public int DriverId { get; set; }

    [Required]
    [Display(Name = "Departure Time")]
    public DateTime DepartureTime { get; set; }

    [Required]
    [Display(Name = "Arrival Time")]
    public DateTime ArrivalTime { get; set; }

    [Required]
    [Display(Name = "Schedule Date")]
    public DateTime ScheduleDate { get; set; }

    [StringLength(20)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled, Delayed

    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated Date")]
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties
    public virtual Bus Bus { get; set; } = null!;
    public virtual Route Route { get; set; } = null!;
    public virtual Driver Driver { get; set; } = null!;
}
