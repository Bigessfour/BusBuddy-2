using BusBuddy.Core.Models;
using BusBuddy.WPF.Models;
using BusBuddy.WPF.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// View model for Route data with validation
    /// </summary>
    public class RouteViewModel : ObservableValidator
    {
        private int _routeId;
        private DateTime _date = DateTime.Today;
        private string _routeName = string.Empty;
        private string? _description;
        private bool _isActive = true;
        private TimeSlot _timeSlot = TimeSlot.AM;
        private int? _vehicleId;
        private decimal? _beginMiles;
        private decimal? _endMiles;
        private int? _riders;
        private int? _driverId;
        private TimeSpan? _beginTime;
        private decimal? _distance;
        private int? _estimatedDuration;
        private int? _studentCount;
        private int? _stopCount;

        [Key]
        public int RouteId
        {
            get => _routeId;
            set => SetProperty(ref _routeId, value);
        }

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date")]
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value, true);
        }

        [Required(ErrorMessage = "Route name is required")]
        [StringLength(50, ErrorMessage = "Route name cannot exceed 50 characters")]
        [Display(Name = "Route Name")]
        public string RouteName
        {
            get => _routeName;
            set => SetProperty(ref _routeName, value, true);
        }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        [Display(Name = "Active")]
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        [Required(ErrorMessage = "Time slot is required")]
        [Display(Name = "Time Slot")]
        public TimeSlot TimeSlot
        {
            get => _timeSlot;
            set => SetProperty(ref _timeSlot, value, true);
        }

        [Display(Name = "Vehicle")]
        public int? VehicleId
        {
            get => _vehicleId;
            set => SetProperty(ref _vehicleId, value);
        }

        [Range(0, 999999.99, ErrorMessage = "Begin miles must be between 0 and 999,999.99")]
        [Display(Name = "Begin Miles")]
        public decimal? BeginMiles
        {
            get => _beginMiles;
            set => SetProperty(ref _beginMiles, value, true);
        }

        [Range(0, 999999.99, ErrorMessage = "End miles must be between 0 and 999,999.99")]
        [CustomValidation(typeof(RouteViewModel), nameof(ValidateEndMiles))]
        [Display(Name = "End Miles")]
        public decimal? EndMiles
        {
            get => _endMiles;
            set => SetProperty(ref _endMiles, value, true);
        }

        [Range(0, 100, ErrorMessage = "Riders must be between 0 and 100")]
        [Display(Name = "Riders")]
        public int? Riders
        {
            get => _riders;
            set => SetProperty(ref _riders, value);
        }

        [Display(Name = "Driver")]
        public int? DriverId
        {
            get => _driverId;
            set => SetProperty(ref _driverId, value);
        }

        [Display(Name = "Begin Time")]
        public TimeSpan? BeginTime
        {
            get => _beginTime;
            set => SetProperty(ref _beginTime, value);
        }

        [Range(0, 9999.99, ErrorMessage = "Distance must be between 0 and 9,999.99 miles")]
        [Display(Name = "Distance (Miles)")]
        public decimal? Distance
        {
            get => _distance;
            set => SetProperty(ref _distance, value);
        }

        [Range(0, 1440, ErrorMessage = "Duration must be between 0 and 1440 minutes (24 hours)")]
        [Display(Name = "Estimated Duration (Minutes)")]
        public int? EstimatedDuration
        {
            get => _estimatedDuration;
            set => SetProperty(ref _estimatedDuration, value);
        }

        [Range(0, 100, ErrorMessage = "Student count must be between 0 and 100")]
        [Display(Name = "Student Count")]
        public int? StudentCount
        {
            get => _studentCount;
            set => SetProperty(ref _studentCount, value);
        }

        [Range(0, 50, ErrorMessage = "Stop count must be between 0 and 50")]
        [Display(Name = "Stop Count")]
        public int? StopCount
        {
            get => _stopCount;
            set => SetProperty(ref _stopCount, value);
        }

        // Formatting properties for UI display
        [Display(Name = "Formatted Date")]
        public string FormattedDate => FormatUtils.FormatDate(Date);

        [Display(Name = "Begin Time")]
        public string FormattedBeginTime => FormatUtils.FormatTime(BeginTime);

        [Display(Name = "Miles")]
        public string FormattedDistance => FormatUtils.FormatMileage(Distance);

        [Display(Name = "Duration")]
        public string FormattedDuration => FormatUtils.FormatDuration(EstimatedDuration);

        [Display(Name = "Time Slot Display")]
        public string TimeSlotDisplay => TimeSlot.GetDisplayName();

        // Calculated properties
        [Display(Name = "Calculated Distance")]
        public decimal? CalculatedDistance
        {
            get
            {
                if (BeginMiles.HasValue && EndMiles.HasValue)
                    return EndMiles.Value - BeginMiles.Value;
                return null;
            }
        }

        // Validation methods
        public static ValidationResult? ValidateEndMiles(decimal? endMiles, ValidationContext context)
        {
            if (context.ObjectInstance is RouteViewModel route &&
                endMiles.HasValue && route.BeginMiles.HasValue &&
                endMiles.Value < route.BeginMiles.Value)
            {
                return new ValidationResult("End miles cannot be less than begin miles");
            }
            return ValidationResult.Success;
        }

        // Conversion methods for Route entity
        public static RouteViewModel FromRoute(Route route)
        {
            var viewModel = new RouteViewModel
            {
                RouteId = route.RouteId,
                Date = route.Date,
                RouteName = route.RouteName,
                Description = route.Description,
                IsActive = route.IsActive,
                Distance = route.Distance,
                EstimatedDuration = route.EstimatedDuration,
                StudentCount = route.StudentCount,
                StopCount = route.StopCount
            };

            // Determine time slot from route data structure
            if (route.AMVehicleId.HasValue || route.AMDriverId.HasValue || route.AMBeginTime.HasValue)
            {
                viewModel.TimeSlot = TimeSlot.AM;
                viewModel.VehicleId = route.AMVehicleId;
                viewModel.DriverId = route.AMDriverId;
                viewModel.BeginMiles = route.AMBeginMiles;
                viewModel.EndMiles = route.AMEndMiles;
                viewModel.Riders = route.AMRiders;
                viewModel.BeginTime = route.AMBeginTime;
            }
            else
            {
                viewModel.TimeSlot = TimeSlot.PM;
                viewModel.VehicleId = route.PMVehicleId;
                viewModel.DriverId = route.PMDriverId;
                viewModel.BeginMiles = route.PMBeginMiles;
                viewModel.EndMiles = route.PMEndMiles;
                viewModel.Riders = route.PMRiders;
                viewModel.BeginTime = route.PMBeginTime;
            }

            return viewModel;
        }

        public Route ToRoute()
        {
            var route = new Route
            {
                RouteId = RouteId,
                Date = Date,
                RouteName = RouteName,
                Description = Description,
                IsActive = IsActive,
                Distance = Distance,
                EstimatedDuration = EstimatedDuration,
                StudentCount = StudentCount,
                StopCount = StopCount
            };

            // Map to AM or PM fields based on time slot
            if (TimeSlot == TimeSlot.AM)
            {
                route.AMVehicleId = VehicleId;
                route.AMDriverId = DriverId;
                route.AMBeginMiles = BeginMiles;
                route.AMEndMiles = EndMiles;
                route.AMRiders = Riders;
                route.AMBeginTime = BeginTime;
            }
            else // PM or other slots
            {
                route.PMVehicleId = VehicleId;
                route.PMDriverId = DriverId;
                route.PMBeginMiles = BeginMiles;
                route.PMEndMiles = EndMiles;
                route.PMRiders = Riders;
                route.PMBeginTime = BeginTime;
            }

            return route;
        }
    }
}
