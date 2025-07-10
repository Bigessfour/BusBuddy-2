using BusBuddy.Core.Models;
using BusBuddy.WPF.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// View model for Driver data with validation
    /// </summary>
    public class DriverViewModel : ObservableValidator
    {
        private int _driverId;
        private string _driverName = string.Empty;
        private string _driverPhone = string.Empty;
        private string _driverEmail = string.Empty;
        private string _driversLicenseType = string.Empty;
        private string _status = "Active";
        private bool _trainingComplete;
        private DateTime? _licenseExpiryDate;
        private string _emergencyContactName = string.Empty;
        private string _emergencyContactPhone = string.Empty;
        private DateTime? _dateOfHire;
        private string _notes = string.Empty;

        [Key]
        public int DriverId
        {
            get => _driverId;
            set => SetProperty(ref _driverId, value);
        }

        [Required(ErrorMessage = "Driver name is required")]
        [StringLength(100, ErrorMessage = "Driver name cannot exceed 100 characters")]
        [Display(Name = "Driver Name")]
        public string DriverName
        {
            get => _driverName;
            set => SetProperty(ref _driverName, value, true);
        }

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string DriverPhone
        {
            get => _driverPhone;
            set => SetProperty(ref _driverPhone, value, true);
        }

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters")]
        [Display(Name = "Email")]
        public string DriverEmail
        {
            get => _driverEmail;
            set => SetProperty(ref _driverEmail, value, true);
        }

        [Required(ErrorMessage = "License type is required")]
        [StringLength(20, ErrorMessage = "License type cannot exceed 20 characters")]
        [Display(Name = "License Type")]
        public string DriversLicenseType
        {
            get => _driversLicenseType;
            set => SetProperty(ref _driversLicenseType, value, true);
        }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [Display(Name = "Status")]
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value, true);
        }

        [Display(Name = "Training Complete")]
        public bool TrainingComplete
        {
            get => _trainingComplete;
            set => SetProperty(ref _trainingComplete, value);
        }

        [Display(Name = "License Expiry Date")]
        [CustomValidation(typeof(DriverViewModel), nameof(ValidateLicenseExpiryDate))]
        public DateTime? LicenseExpiryDate
        {
            get => _licenseExpiryDate;
            set => SetProperty(ref _licenseExpiryDate, value, true);
        }

        [StringLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        [Display(Name = "Emergency Contact")]
        public string EmergencyContactName
        {
            get => _emergencyContactName;
            set => SetProperty(ref _emergencyContactName, value);
        }

        [Phone(ErrorMessage = "Please enter a valid emergency contact phone number")]
        [StringLength(20, ErrorMessage = "Emergency contact phone cannot exceed 20 characters")]
        [Display(Name = "Emergency Phone")]
        public string EmergencyContactPhone
        {
            get => _emergencyContactPhone;
            set => SetProperty(ref _emergencyContactPhone, value);
        }

        [Display(Name = "Date of Hire")]
        public DateTime? DateOfHire
        {
            get => _dateOfHire;
            set => SetProperty(ref _dateOfHire, value);
        }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        // Formatting properties for UI display
        [Display(Name = "Formatted Phone")]
        public string FormattedPhone => FormatUtils.FormatPhoneNumber(DriverPhone);

        [Display(Name = "Formatted Emergency Phone")]
        public string FormattedEmergencyPhone => FormatUtils.FormatPhoneNumber(EmergencyContactPhone);

        [Display(Name = "License Expiry")]
        public string FormattedLicenseExpiry => FormatUtils.FormatDate(LicenseExpiryDate);

        [Display(Name = "Hire Date")]
        public string FormattedHireDate => FormatUtils.FormatDate(DateOfHire);

        // Computed properties
        [Display(Name = "License Status")]
        public string LicenseStatus
        {
            get
            {
                if (!LicenseExpiryDate.HasValue) return "Unknown";
                var daysUntilExpiry = (LicenseExpiryDate.Value - DateTime.Now).Days;
                return daysUntilExpiry switch
                {
                    < 0 => "Expired",
                    < 30 => "Expiring Soon",
                    _ => "Valid"
                };
            }
        }

        [Display(Name = "Tenure")]
        public string Tenure
        {
            get
            {
                if (!DateOfHire.HasValue) return "Unknown";
                var years = (DateTime.Now - DateOfHire.Value).TotalDays / 365.25;

                if (years < 1)
                    return $"{Math.Floor(years * 12)} months";

                return $"{Math.Floor(years)} years";
            }
        }

        [Display(Name = "Is Available")]
        public bool IsAvailable => Status == "Active";

        [Display(Name = "Needs Attention")]
        public bool NeedsAttention => LicenseStatus == "Expired" || LicenseStatus == "Expiring Soon" || !TrainingComplete;

        // Validation methods
        public static ValidationResult? ValidateLicenseExpiryDate(DateTime? date, ValidationContext context)
        {
            if (date.HasValue && date.Value < DateTime.Today)
            {
                return new ValidationResult("License has expired. Please renew before assigning to routes.");
            }
            return ValidationResult.Success;
        }

        // Conversion methods
        public static DriverViewModel FromDriver(Driver driver)
        {
            return new DriverViewModel
            {
                DriverId = driver.DriverId,
                DriverName = driver.DriverName,
                DriverPhone = driver.DriverPhone ?? string.Empty,
                DriverEmail = driver.DriverEmail ?? string.Empty,
                DriversLicenseType = driver.DriversLicenceType ?? string.Empty,
                Status = driver.Status ?? "Active",
                TrainingComplete = driver.TrainingComplete,
                LicenseExpiryDate = driver.LicenseExpiryDate,
                EmergencyContactName = driver.EmergencyContactName ?? string.Empty,
                EmergencyContactPhone = driver.EmergencyContactPhone ?? string.Empty,
                DateOfHire = driver.HireDate,
                Notes = driver.Notes ?? string.Empty
            };
        }

        public Driver ToDriver()
        {
            return new Driver
            {
                DriverId = DriverId,
                DriverName = DriverName,
                DriverPhone = DriverPhone,
                DriverEmail = DriverEmail,
                DriversLicenceType = DriversLicenseType,
                Status = Status,
                TrainingComplete = TrainingComplete,
                LicenseExpiryDate = LicenseExpiryDate,
                EmergencyContactName = EmergencyContactName,
                EmergencyContactPhone = EmergencyContactPhone,
                HireDate = DateOfHire,
                Notes = Notes
            };
        }
    }
}
