using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Buddy.Models;

/// <summary>
/// Represents a school bus vehicle in the fleet
/// Based on Vehicle Table from BusBuddy Tables schema
/// Matches requirements: Bus #, Year, Make, Model, Seating Capacity, VIN Number, License Number, Date Last Inspection
/// Enhanced for Syncfusion data binding with INotifyPropertyChanged support
/// </summary>
[Table("Vehicles")]
public class Bus : INotifyPropertyChanged
{
    private string _busNumber = string.Empty;
    private int _year;
    private string _make = string.Empty;
    private string _model = string.Empty;
    private int _seatingCapacity;
    private string _vinNumber = string.Empty;
    private string _licenseNumber = string.Empty;
    private DateTime? _dateLastInspection;
    private int? _currentOdometer;
    private string _status = "Active";
    private DateTime? _purchaseDate;
    private decimal? _purchasePrice;
    private string? _insurancePolicyNumber;
    private DateTime? _insuranceExpiryDate;

    [Key]
    public int VehicleId { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Bus #")]
    public string BusNumber
    {
        get => _busNumber;
        set
        {
            if (_busNumber != value)
            {
                _busNumber = value ?? string.Empty;
                OnPropertyChanged(nameof(BusNumber));
            }
        }
    }

    [Required]
    [Range(1900, 2030)]
    [Display(Name = "Year")]
    public int Year
    {
        get => _year;
        set
        {
            if (_year != value)
            {
                _year = value;
                OnPropertyChanged(nameof(Year));
                OnPropertyChanged(nameof(Age));
            }
        }
    }

    [Required]
    [StringLength(50)]
    [Display(Name = "Make")]
    public string Make
    {
        get => _make;
        set
        {
            if (_make != value)
            {
                _make = value ?? string.Empty;
                OnPropertyChanged(nameof(Make));
                OnPropertyChanged(nameof(FullDescription));
            }
        }
    }

    [Required]
    [StringLength(50)]
    [Display(Name = "Model")]
    public string Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                _model = value ?? string.Empty;
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(FullDescription));
            }
        }
    }

    [Required]
    [Range(1, 90)]
    [Display(Name = "Seating Capacity")]
    public int SeatingCapacity
    {
        get => _seatingCapacity;
        set
        {
            if (_seatingCapacity != value)
            {
                _seatingCapacity = value;
                OnPropertyChanged(nameof(SeatingCapacity));
            }
        }
    }

    [Required]
    [StringLength(17)]
    [Display(Name = "VIN Number")]
    public string VINNumber
    {
        get => _vinNumber;
        set
        {
            if (_vinNumber != value)
            {
                _vinNumber = value ?? string.Empty;
                OnPropertyChanged(nameof(VINNumber));
            }
        }
    }

    [Required]
    [StringLength(20)]
    [Display(Name = "License Number")]
    public string LicenseNumber
    {
        get => _licenseNumber;
        set
        {
            if (_licenseNumber != value)
            {
                _licenseNumber = value ?? string.Empty;
                OnPropertyChanged(nameof(LicenseNumber));
            }
        }
    }

    [Display(Name = "Date Last Inspection")]
    public DateTime? DateLastInspection
    {
        get => _dateLastInspection;
        set
        {
            if (_dateLastInspection != value)
            {
                _dateLastInspection = value;
                OnPropertyChanged(nameof(DateLastInspection));
                OnPropertyChanged(nameof(InspectionStatus));
            }
        }
    }

    [Display(Name = "Current Odometer")]
    public int? CurrentOdometer
    {
        get => _currentOdometer;
        set
        {
            if (_currentOdometer != value)
            {
                _currentOdometer = value;
                OnPropertyChanged(nameof(CurrentOdometer));
            }
        }
    }

    [StringLength(20)]
    [Display(Name = "Current Status")]
    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value ?? "Active";
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    [Display(Name = "Purchase Date")]
    public DateTime? PurchaseDate
    {
        get => _purchaseDate;
        set
        {
            if (_purchaseDate != value)
            {
                _purchaseDate = value;
                OnPropertyChanged(nameof(PurchaseDate));
            }
        }
    }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Purchase Price")]
    public decimal? PurchasePrice
    {
        get => _purchasePrice;
        set
        {
            if (_purchasePrice != value)
            {
                _purchasePrice = value;
                OnPropertyChanged(nameof(PurchasePrice));
            }
        }
    }

    [StringLength(100)]
    [Display(Name = "Insurance Policy")]
    public string? InsurancePolicyNumber
    {
        get => _insurancePolicyNumber;
        set
        {
            if (_insurancePolicyNumber != value)
            {
                _insurancePolicyNumber = value;
                OnPropertyChanged(nameof(InsurancePolicyNumber));
            }
        }
    }

    [Display(Name = "Insurance Expiry")]
    public DateTime? InsuranceExpiryDate
    {
        get => _insuranceExpiryDate;
        set
        {
            if (_insuranceExpiryDate != value)
            {
                _insuranceExpiryDate = value;
                OnPropertyChanged(nameof(InsuranceExpiryDate));
                OnPropertyChanged(nameof(InsuranceStatus));
            }
        }
    }

    // Extended properties for maximum flexibility
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

    [StringLength(50)]
    [Display(Name = "Department")]
    public string? Department { get; set; }

    [StringLength(20)]
    [Display(Name = "Fleet Type")]
    public string? FleetType { get; set; } // Regular, Special Needs, Activity, etc.

    [Display(Name = "Fuel Capacity")]
    [Column(TypeName = "decimal(8,2)")]
    public decimal? FuelCapacity { get; set; }

    [StringLength(20)]
    [Display(Name = "Fuel Type")]
    public string? FuelType { get; set; } // Gasoline, Diesel, Electric, Hybrid

    [Display(Name = "Miles Per Gallon")]
    [Column(TypeName = "decimal(6,2)")]
    public decimal? MilesPerGallon { get; set; }

    [Display(Name = "Next Maintenance Due")]
    public DateTime? NextMaintenanceDue { get; set; }

    [Display(Name = "Next Maintenance Mileage")]
    public int? NextMaintenanceMileage { get; set; }

    [Display(Name = "Last Service Date")]
    public DateTime? LastServiceDate { get; set; }

    [StringLength(1000)]
    [Display(Name = "Special Equipment")]
    public string? SpecialEquipment { get; set; } // Wheelchair lift, Air conditioning, etc.

    [Display(Name = "GPS Tracking")]
    public bool GPSTracking { get; set; } = false;

    [StringLength(100)]
    [Display(Name = "GPS Device ID")]
    public string? GPSDeviceId { get; set; }

    [StringLength(1000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    // Computed properties for UI and reporting
    [NotMapped]
    [Display(Name = "Age")]
    public int Age => DateTime.Now.Year - Year;

    [NotMapped]
    [Display(Name = "Full Description")]
    public string FullDescription => $"{Year} {Make} {Model} (#{BusNumber})";

    [NotMapped]
    [Display(Name = "Inspection Status")]
    public string InspectionStatus
    {
        get
        {
            if (!DateLastInspection.HasValue) return "Overdue";
            var daysSinceInspection = (DateTime.Now - DateLastInspection.Value).TotalDays;
            var monthsSinceInspection = daysSinceInspection / 30.0; // Use double for precision
            return monthsSinceInspection switch
            {
                > 12.0 => "Overdue",
                > 11.0 => "Due Soon",
                _ => "Current"
            };
        }
    }

    [NotMapped]
    [Display(Name = "Insurance Status")]
    public string InsuranceStatus
    {
        get
        {
            if (!InsuranceExpiryDate.HasValue) return "Unknown";
            var daysUntilExpiry = (InsuranceExpiryDate.Value - DateTime.Now).Days;
            return daysUntilExpiry switch
            {
                < 0 => "Expired",
                < 30 => "Expiring Soon",
                _ => "Current"
            };
        }
    }

    [NotMapped]
    [Display(Name = "Is Available")]
    public bool IsAvailable => Status == "Active";

    [NotMapped]
    [Display(Name = "Needs Attention")]
    public bool NeedsAttention => InspectionStatus != "Current" || InsuranceStatus == "Expired" || InsuranceStatus == "Expiring Soon";

    // Navigation properties
    public virtual ICollection<Route> AMRoutes { get; set; } = new List<Route>();
    public virtual ICollection<Route> PMRoutes { get; set; } = new List<Route>();
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public virtual ICollection<ActivitySchedule> ScheduledActivities { get; set; } = new List<ActivitySchedule>();
    public virtual ICollection<Fuel> FuelRecords { get; set; } = new List<Fuel>();
    public virtual ICollection<Maintenance> MaintenanceRecords { get; set; } = new List<Maintenance>();

    // Legacy navigation for backward compatibility
    [NotMapped]
    public virtual ICollection<Route> Routes => AMRoutes.Concat(PMRoutes).ToList();

    // INotifyPropertyChanged implementation for Syncfusion data binding
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
