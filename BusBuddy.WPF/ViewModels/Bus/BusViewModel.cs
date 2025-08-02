using BusBuddy.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// View model for Bus/Vehicle data with validation
    /// </summary>
    public class BusViewModel : ObservableValidator
    {
        private int _vehicleId;
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
        private DateTime? _insuranceExpiryDate;

        [Key]
        public int VehicleId
        {
            get => _vehicleId;
            set => SetProperty(ref _vehicleId, value);
        }

        [Required(ErrorMessage = "Bus number is required")]
        [StringLength(20, ErrorMessage = "Bus number cannot exceed 20 characters")]
        [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Bus number must contain only letters, numbers, and hyphens")]
        [Display(Name = "Bus #")]
        public string BusNumber
        {
            get => _busNumber;
            set => SetProperty(ref _busNumber, value, true);
        }

        [Required(ErrorMessage = "Year is required")]
        [Range(1980, 2030, ErrorMessage = "Year must be between 1980 and 2030")]
        [Display(Name = "Year")]
        public int Year
        {
            get => _year;
            set => SetProperty(ref _year, value, true);
        }

        [Required(ErrorMessage = "Make is required")]
        [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters")]
        [Display(Name = "Make")]
        public string Make
        {
            get => _make;
            set => SetProperty(ref _make, value, true);
        }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
        [Display(Name = "Model")]
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value, true);
        }

        [Required(ErrorMessage = "Seating capacity is required")]
        [Range(1, 90, ErrorMessage = "Seating capacity must be between 1 and 90")]
        [Display(Name = "Seating Capacity")]
        public int SeatingCapacity
        {
            get => _seatingCapacity;
            set => SetProperty(ref _seatingCapacity, value, true);
        }

        [Required(ErrorMessage = "VIN number is required")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN number must be exactly 17 characters")]
        [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$", ErrorMessage = "VIN number contains invalid characters")]
        [Display(Name = "VIN Number")]
        public string VINNumber
        {
            get => _vinNumber;
            set => SetProperty(ref _vinNumber, value != null ? value.ToUpper() : string.Empty, true);
        }

        [Required(ErrorMessage = "License number is required")]
        [StringLength(20, ErrorMessage = "License number cannot exceed 20 characters")]
        [Display(Name = "License Number")]
        public string LicenseNumber
        {
            get => _licenseNumber;
            set => SetProperty(ref _licenseNumber, value, true);
        }

        [Display(Name = "Last Inspection")]
        public DateTime? DateLastInspection
        {
            get => _dateLastInspection;
            set => SetProperty(ref _dateLastInspection, value);
        }

        [Range(0, 1000000, ErrorMessage = "Odometer reading must be between 0 and 1,000,000")]
        [Display(Name = "Current Odometer")]
        public int? CurrentOdometer
        {
            get => _currentOdometer;
            set => SetProperty(ref _currentOdometer, value);
        }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [Display(Name = "Status")]
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value, true);
        }

        [Display(Name = "Insurance Expiry")]
        [CustomValidation(typeof(BusViewModel), nameof(ValidateInsuranceDate))]
        public DateTime? InsuranceExpiryDate
        {
            get => _insuranceExpiryDate;
            set => SetProperty(ref _insuranceExpiryDate, value, true);
        }

        // Computed properties with formatting
        [Display(Name = "Age")]
        public int Age => DateTime.Now.Year - Year;

        [Display(Name = "Full Description")]
        public string FullDescription => $"{Year} {Make} {Model} (#{BusNumber})";

        [Display(Name = "Inspection Status")]
        public string InspectionStatus
        {
            get
            {
                if (!DateLastInspection.HasValue)
                {
                    return "Overdue";
                }


                var daysSinceInspection = (DateTime.Now - DateLastInspection.Value).TotalDays;
                var monthsSinceInspection = daysSinceInspection / 30.0;
                return monthsSinceInspection switch
                {
                    > 12.0 => "Overdue",
                    > 11.0 => "Due Soon",
                    _ => "Current"
                };
            }
        }

        [Display(Name = "Insurance Status")]
        public string InsuranceStatus
        {
            get
            {
                if (!InsuranceExpiryDate.HasValue)
                {
                    return "Unknown";
                }


                var daysUntilExpiry = (InsuranceExpiryDate.Value - DateTime.Now).Days;
                return daysUntilExpiry switch
                {
                    < 0 => "Expired",
                    < 30 => "Expiring Soon",
                    _ => "Current"
                };
            }
        }

        // Validation methods
        public static ValidationResult? ValidateInsuranceDate(DateTime? date, ValidationContext context)
        {
            if (date.HasValue && date.Value < DateTime.Today)
            {
                return new ValidationResult("Insurance expiry date cannot be in the past");
            }
            return ValidationResult.Success;
        }

        // Conversion methods
        public static BusViewModel FromBus(Bus bus)
        {
            return new BusViewModel
            {
                VehicleId = bus.VehicleId,
                BusNumber = bus.BusNumber,
                Year = bus.Year,
                Make = bus.Make,
                Model = bus.Model,
                SeatingCapacity = bus.SeatingCapacity,
                VINNumber = bus.VINNumber,
                LicenseNumber = bus.LicenseNumber,
                DateLastInspection = bus.DateLastInspection,
                CurrentOdometer = bus.CurrentOdometer,
                Status = bus.Status,
                InsuranceExpiryDate = bus.InsuranceExpiryDate
            };
        }

        public Bus ToBus()
        {
            return new Bus
            {
                VehicleId = VehicleId,
                BusNumber = BusNumber,
                Year = Year,
                Make = Make,
                Model = Model,
                SeatingCapacity = SeatingCapacity,
                VINNumber = VINNumber,
                LicenseNumber = LicenseNumber,
                DateLastInspection = DateLastInspection,
                CurrentOdometer = CurrentOdometer,
                Status = Status,
                InsuranceExpiryDate = InsuranceExpiryDate
            };
        }
    }
}
