using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.WPF.Models;
using Serilog;

namespace BusBuddy.WPF.Services
{
    /// <summary>
    /// Service for validating data integrity across all transportation entities
    /// Provides comprehensive validation for routes, activities, students, drivers, and vehicles
    /// </summary>
    public class DataIntegrityService : IDataIntegrityService
    {
        private static readonly ILogger Logger = Log.ForContext<DataIntegrityService>();
        private readonly IRouteService _routeService;
        private readonly IDriverService _driverService;
        private readonly IVehicleService _vehicleService;
        private readonly IActivityService _activityService;
        private readonly IStudentService _studentService;

        public DataIntegrityService(
            IRouteService routeService,
            IDriverService driverService,
            IVehicleService vehicleService,
            IActivityService activityService,
            IStudentService studentService)
        {
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        }

        /// <summary>
        /// Perform comprehensive data integrity validation across all entities
        /// </summary>
        public async Task<DataIntegrityReport> ValidateAllDataAsync()
        {
            Logger.Information("Starting comprehensive data integrity validation");
            var report = new DataIntegrityReport();

            try
            {
                // Validate each entity type
                var routeValidation = await ValidateRoutesAsync();
                var activityValidation = await ValidateActivitiesAsync();
                var studentValidation = await ValidateStudentsAsync();
                var driverValidation = await ValidateDriversAsync();
                var vehicleValidation = await ValidateVehiclesAsync();
                var crossValidation = await ValidateCrossEntityRelationshipsAsync();

                // Combine all validation results
                report.RouteIssues = routeValidation;
                report.ActivityIssues = activityValidation;
                report.StudentIssues = studentValidation;
                report.DriverIssues = driverValidation;
                report.VehicleIssues = vehicleValidation;
                report.CrossEntityIssues = crossValidation;

                report.TotalIssuesFound =
                    routeValidation.Count +
                    activityValidation.Count +
                    studentValidation.Count +
                    driverValidation.Count +
                    vehicleValidation.Count +
                    crossValidation.Count;

                Logger.Information("Data integrity validation completed. Total issues found: {IssueCount}", report.TotalIssuesFound);
                return report;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to complete data integrity validation");
                report.ValidationError = ex.Message;
                return report;
            }
        }

        /// <summary>
        /// Validate route data integrity
        /// </summary>
        public async Task<List<DataIntegrityIssue>> ValidateRoutesAsync()
        {
            var issues = new List<DataIntegrityIssue>();

            try
            {
                var routes = await _routeService.GetAllRoutesAsync();

                foreach (var route in routes)
                {
                    // Validate basic route properties
                    if (string.IsNullOrWhiteSpace(route.RouteName))
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Route",
                            EntityId = route.RouteId.ToString(),
                            IssueType = "Missing Required Data",
                            Description = "Route name is null or empty",
                            Severity = "High"
                        });
                    }

                    // Validate route number format
                    if (route.RouteNumber <= 0)
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Route",
                            EntityId = route.RouteId.ToString(),
                            IssueType = "Invalid Data Format",
                            Description = "Route number must be greater than 0",
                            Severity = "High"
                        });
                    }

                    // Validate route stops
                    if (route.Stops == null || !route.Stops.Any())
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Route",
                            EntityId = route.RouteId.ToString(),
                            IssueType = "Missing Required Data",
                            Description = "Route has no stops defined",
                            Severity = "Critical"
                        });
                    }
                    else
                    {
                        // Validate individual stops
                        foreach (var stop in route.Stops)
                        {
                            if (string.IsNullOrWhiteSpace(stop.StopName))
                            {
                                issues.Add(new DataIntegrityIssue
                                {
                                    EntityType = "Route",
                                    EntityId = route.RouteId.ToString(),
                                    IssueType = "Missing Required Data",
                                    Description = $"Stop at position {stop.StopOrder} has no name",
                                    Severity = "Medium"
                                });
                            }

                            if (stop.Latitude == 0 && stop.Longitude == 0)
                            {
                                issues.Add(new DataIntegrityIssue
                                {
                                    EntityType = "Route",
                                    EntityId = route.RouteId.ToString(),
                                    IssueType = "Missing Required Data",
                                    Description = $"Stop '{stop.StopName}' has no GPS coordinates",
                                    Severity = "Medium"
                                });
                            }
                        }
                    }

                    // Validate route schedule consistency
                    if (route.StartTime >= route.EndTime)
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Route",
                            EntityId = route.RouteId.ToString(),
                            IssueType = "Business Logic Violation",
                            Description = "Route start time must be before end time",
                            Severity = "High"
                        });
                    }
                }

                // Check for duplicate route numbers
                var duplicateRouteNumbers = routes
                    .GroupBy(r => r.RouteNumber)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                foreach (var duplicateNumber in duplicateRouteNumbers)
                {
                    issues.Add(new DataIntegrityIssue
                    {
                        EntityType = "Route",
                        EntityId = "Multiple",
                        IssueType = "Duplicate Data",
                        Description = $"Route number {duplicateNumber} is used by multiple routes",
                        Severity = "High"
                    });
                }

                Logger.Information("Route validation completed. Found {IssueCount} issues", issues.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during route validation");
                issues.Add(new DataIntegrityIssue
                {
                    EntityType = "Route",
                    EntityId = "System",
                    IssueType = "Validation Error",
                    Description = $"Route validation failed: {ex.Message}",
                    Severity = "Critical"
                });
            }

            return issues;
        }

        /// <summary>
        /// Validate activity data integrity
        /// </summary>
        public async Task<List<DataIntegrityIssue>> ValidateActivitiesAsync()
        {
            var issues = new List<DataIntegrityIssue>();

            try
            {
                var activities = await _activityService.GetAllActivitiesAsync();
                var drivers = await _driverService.GetAllDriversAsync();
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                var routes = await _routeService.GetAllRoutesAsync();

                foreach (var activity in activities)
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(activity.ActivityType))
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Activity",
                            EntityId = activity.ActivityId.ToString(),
                            IssueType = "Missing Required Data",
                            Description = "Activity type is null or empty",
                            Severity = "High"
                        });
                    }

                    // Validate date logic
                    if (activity.StartTime >= activity.EndTime)
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Activity",
                            EntityId = activity.ActivityId.ToString(),
                            IssueType = "Business Logic Violation",
                            Description = "Activity start time must be before end time",
                            Severity = "High"
                        });
                    }

                    // Validate future dates for scheduled activities
                    if (activity.Status == "Scheduled" && activity.StartTime < DateTime.Now)
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Activity",
                            EntityId = activity.ActivityId.ToString(),
                            IssueType = "Business Logic Violation",
                            Description = "Scheduled activity cannot have start time in the past",
                            Severity = "Medium"
                        });
                    }

                    // Validate driver assignment
                    if (activity.DriverId.HasValue)
                    {
                        var assignedDriver = drivers.FirstOrDefault(d => d.DriverId == activity.DriverId.Value);
                        if (assignedDriver == null)
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Activity",
                                EntityId = activity.ActivityId.ToString(),
                                IssueType = "Reference Integrity",
                                Description = $"Assigned driver ID {activity.DriverId} does not exist",
                                Severity = "High"
                            });
                        }
                        else if (assignedDriver.Status != "Active")
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Activity",
                                EntityId = activity.ActivityId.ToString(),
                                IssueType = "Business Logic Violation",
                                Description = $"Driver {assignedDriver.FullName} is not active",
                                Severity = "Medium"
                            });
                        }
                    }

                    // Validate vehicle assignment
                    if (activity.VehicleId.HasValue)
                    {
                        var assignedVehicle = vehicles.FirstOrDefault(v => v.VehicleId == activity.VehicleId.Value);
                        if (assignedVehicle == null)
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Activity",
                                EntityId = activity.ActivityId.ToString(),
                                IssueType = "Reference Integrity",
                                Description = $"Assigned vehicle ID {activity.VehicleId} does not exist",
                                Severity = "High"
                            });
                        }
                        else if (assignedVehicle.Status != "Active")
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Activity",
                                EntityId = activity.ActivityId.ToString(),
                                IssueType = "Business Logic Violation",
                                Description = $"Vehicle {assignedVehicle.VehicleNumber} is not active",
                                Severity = "Medium"
                            });
                        }
                    }

                    // Validate route assignment for route-based activities
                    if (activity.RouteId.HasValue)
                    {
                        var assignedRoute = routes.FirstOrDefault(r => r.RouteId == activity.RouteId.Value);
                        if (assignedRoute == null)
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Activity",
                                EntityId = activity.ActivityId.ToString(),
                                IssueType = "Reference Integrity",
                                Description = $"Assigned route ID {activity.RouteId} does not exist",
                                Severity = "High"
                            });
                        }
                    }
                }

                Logger.Information("Activity validation completed. Found {IssueCount} issues", issues.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during activity validation");
                issues.Add(new DataIntegrityIssue
                {
                    EntityType = "Activity",
                    EntityId = "System",
                    IssueType = "Validation Error",
                    Description = $"Activity validation failed: {ex.Message}",
                    Severity = "Critical"
                });
            }

            return issues;
        }

        /// <summary>
        /// Validate student data integrity
        /// </summary>
        public async Task<List<DataIntegrityIssue>> ValidateStudentsAsync()
        {
            var issues = new List<DataIntegrityIssue>();

            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                var routes = await _routeService.GetAllRoutesAsync();

                foreach (var student in students)
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(student.FirstName))
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Student",
                            EntityId = student.StudentId.ToString(),
                            IssueType = "Missing Required Data",
                            Description = "Student first name is null or empty",
                            Severity = "High"
                        });
                    }

                    if (string.IsNullOrWhiteSpace(student.LastName))
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Student",
                            EntityId = student.StudentId.ToString(),
                            IssueType = "Missing Required Data",
                            Description = "Student last name is null or empty",
                            Severity = "High"
                        });
                    }

                    // Validate student ID format (assuming numeric)
                    if (string.IsNullOrWhiteSpace(student.StudentNumber) || !student.StudentNumber.All(char.IsDigit))
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Student",
                            EntityId = student.StudentId.ToString(),
                            IssueType = "Invalid Data Format",
                            Description = "Student number must be numeric",
                            Severity = "Medium"
                        });
                    }

                    // Validate grade level
                    if (student.GradeLevel < 1 || student.GradeLevel > 12)
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Student",
                            EntityId = student.StudentId.ToString(),
                            IssueType = "Invalid Data Format",
                            Description = "Grade level must be between 1 and 12",
                            Severity = "Medium"
                        });
                    }

                    // Validate route assignment
                    if (student.RouteId.HasValue)
                    {
                        var assignedRoute = routes.FirstOrDefault(r => r.RouteId == student.RouteId.Value);
                        if (assignedRoute == null)
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Student",
                                EntityId = student.StudentId.ToString(),
                                IssueType = "Reference Integrity",
                                Description = $"Assigned route ID {student.RouteId} does not exist",
                                Severity = "High"
                            });
                        }
                    }

                    // Validate pickup/dropoff stops
                    if (student.PickupStopId.HasValue && student.RouteId.HasValue)
                    {
                        var route = routes.FirstOrDefault(r => r.RouteId == student.RouteId.Value);
                        if (route != null && !route.Stops.Any(s => s.StopId == student.PickupStopId.Value))
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Student",
                                EntityId = student.StudentId.ToString(),
                                IssueType = "Reference Integrity",
                                Description = "Pickup stop is not on assigned route",
                                Severity = "High"
                            });
                        }
                    }

                    // Validate contact information
                    if (string.IsNullOrWhiteSpace(student.ParentPhone) && string.IsNullOrWhiteSpace(student.EmergencyContact))
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Student",
                            EntityId = student.StudentId.ToString(),
                            IssueType = "Missing Required Data",
                            Description = "Student has no parent phone or emergency contact",
                            Severity = "Critical"
                        });
                    }
                }

                // Check for duplicate student numbers
                var duplicateStudentNumbers = students
                    .Where(s => !string.IsNullOrWhiteSpace(s.StudentNumber))
                    .GroupBy(s => s.StudentNumber)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                foreach (var duplicateNumber in duplicateStudentNumbers)
                {
                    issues.Add(new DataIntegrityIssue
                    {
                        EntityType = "Student",
                        EntityId = "Multiple",
                        IssueType = "Duplicate Data",
                        Description = $"Student number {duplicateNumber} is used by multiple students",
                        Severity = "High"
                    });
                }

                Logger.Information("Student validation completed. Found {IssueCount} issues", issues.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during student validation");
                issues.Add(new DataIntegrityIssue
                {
                    EntityType = "Student",
                    EntityId = "System",
                    IssueType = "Validation Error",
                    Description = $"Student validation failed: {ex.Message}",
                    Severity = "Critical"
                });
            }

            return issues;
        }

        /// <summary>
        /// Validate driver data integrity
        /// </summary>
        public async Task<List<DataIntegrityIssue>> ValidateDriversAsync()
        {
            var issues = new List<DataIntegrityIssue>();

            try
            {
                var drivers = await _driverService.GetAllDriversAsync();

                foreach (var driver in drivers)
                {
                    // Validate license expiration
                    if (driver.LicenseExpirationDate < DateTime.Now.AddDays(30))
                    {
                        var severity = driver.LicenseExpirationDate < DateTime.Now ? "Critical" : "High";
                        var description = driver.LicenseExpirationDate < DateTime.Now
                            ? "Driver license has expired"
                            : "Driver license expires within 30 days";

                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Driver",
                            EntityId = driver.DriverId.ToString(),
                            IssueType = "License Issue",
                            Description = description,
                            Severity = severity
                        });
                    }

                    // Validate contact information
                    if (string.IsNullOrWhiteSpace(driver.PhoneNumber))
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Driver",
                            EntityId = driver.DriverId.ToString(),
                            IssueType = "Missing Required Data",
                            Description = "Driver has no phone number",
                            Severity = "High"
                        });
                    }
                }

                Logger.Information("Driver validation completed. Found {IssueCount} issues", issues.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during driver validation");
                issues.Add(new DataIntegrityIssue
                {
                    EntityType = "Driver",
                    EntityId = "System",
                    IssueType = "Validation Error",
                    Description = $"Driver validation failed: {ex.Message}",
                    Severity = "Critical"
                });
            }

            return issues;
        }

        /// <summary>
        /// Validate vehicle data integrity
        /// </summary>
        public async Task<List<DataIntegrityIssue>> ValidateVehiclesAsync()
        {
            var issues = new List<DataIntegrityIssue>();

            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();

                foreach (var vehicle in vehicles)
                {
                    // Validate inspection dates
                    if (vehicle.LastInspectionDate.HasValue && vehicle.LastInspectionDate < DateTime.Now.AddDays(-365))
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Vehicle",
                            EntityId = vehicle.VehicleId.ToString(),
                            IssueType = "Compliance Issue",
                            Description = "Vehicle inspection is overdue (>365 days)",
                            Severity = "Critical"
                        });
                    }

                    // Validate mileage consistency
                    if (vehicle.Mileage < 0)
                    {
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "Vehicle",
                            EntityId = vehicle.VehicleId.ToString(),
                            IssueType = "Invalid Data Format",
                            Description = "Vehicle mileage cannot be negative",
                            Severity = "High"
                        });
                    }
                }

                Logger.Information("Vehicle validation completed. Found {IssueCount} issues", issues.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during vehicle validation");
                issues.Add(new DataIntegrityIssue
                {
                    EntityType = "Vehicle",
                    EntityId = "System",
                    IssueType = "Validation Error",
                    Description = $"Vehicle validation failed: {ex.Message}",
                    Severity = "Critical"
                });
            }

            return issues;
        }

        /// <summary>
        /// Validate cross-entity relationships and business rules
        /// </summary>
        public async Task<List<DataIntegrityIssue>> ValidateCrossEntityRelationshipsAsync()
        {
            var issues = new List<DataIntegrityIssue>();

            try
            {
                var activities = await _activityService.GetAllActivitiesAsync();
                var drivers = await _driverService.GetAllDriversAsync();
                var vehicles = await _vehicleService.GetAllVehiclesAsync();

                // Check for double-booked drivers
                var driverConflicts = activities
                    .Where(a => a.DriverId.HasValue && a.Status == "Scheduled")
                    .GroupBy(a => new { a.DriverId, Date = a.StartTime.Date })
                    .Where(g => g.Count() > 1);

                foreach (var conflict in driverConflicts)
                {
                    var conflictingActivities = conflict.ToList();
                    foreach (var activity in conflictingActivities)
                    {
                        var overlaps = conflictingActivities
                            .Where(other => other.ActivityId != activity.ActivityId)
                            .Any(other => activity.StartTime < other.EndTime && activity.EndTime > other.StartTime);

                        if (overlaps)
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Activity",
                                EntityId = activity.ActivityId.ToString(),
                                IssueType = "Scheduling Conflict",
                                Description = $"Driver {conflict.Key.DriverId} has overlapping activities",
                                Severity = "High"
                            });
                        }
                    }
                }

                // Check for double-booked vehicles
                var vehicleConflicts = activities
                    .Where(a => a.VehicleId.HasValue && a.Status == "Scheduled")
                    .GroupBy(a => new { a.VehicleId, Date = a.StartTime.Date })
                    .Where(g => g.Count() > 1);

                foreach (var conflict in vehicleConflicts)
                {
                    var conflictingActivities = conflict.ToList();
                    foreach (var activity in conflictingActivities)
                    {
                        var overlaps = conflictingActivities
                            .Where(other => other.ActivityId != activity.ActivityId)
                            .Any(other => activity.StartTime < other.EndTime && activity.EndTime > other.StartTime);

                        if (overlaps)
                        {
                            issues.Add(new DataIntegrityIssue
                            {
                                EntityType = "Activity",
                                EntityId = activity.ActivityId.ToString(),
                                IssueType = "Scheduling Conflict",
                                Description = $"Vehicle {conflict.Key.VehicleId} has overlapping activities",
                                Severity = "High"
                            });
                        }
                    }
                }

                Logger.Information("Cross-entity validation completed. Found {IssueCount} issues", issues.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during cross-entity validation");
                issues.Add(new DataIntegrityIssue
                {
                    EntityType = "System",
                    EntityId = "CrossEntity",
                    IssueType = "Validation Error",
                    Description = $"Cross-entity validation failed: {ex.Message}",
                    Severity = "Critical"
                });
            }

            return issues;
        }

        /// <summary>
        /// Validate specific entity by ID
        /// </summary>
        public async Task<List<DataIntegrityIssue>> ValidateEntityAsync(string entityType, int entityId)
        {
            var issues = new List<DataIntegrityIssue>();

            try
            {
                switch (entityType.ToLowerInvariant())
                {
                    case "route":
                        var routeIssues = await ValidateRoutesAsync();
                        issues.AddRange(routeIssues.Where(i => i.EntityId == entityId.ToString()));
                        break;

                    case "activity":
                        var activityIssues = await ValidateActivitiesAsync();
                        issues.AddRange(activityIssues.Where(i => i.EntityId == entityId.ToString()));
                        break;

                    case "student":
                        var studentIssues = await ValidateStudentsAsync();
                        issues.AddRange(studentIssues.Where(i => i.EntityId == entityId.ToString()));
                        break;

                    case "driver":
                        var driverIssues = await ValidateDriversAsync();
                        issues.AddRange(driverIssues.Where(i => i.EntityId == entityId.ToString()));
                        break;

                    case "vehicle":
                        var vehicleIssues = await ValidateVehiclesAsync();
                        issues.AddRange(vehicleIssues.Where(i => i.EntityId == entityId.ToString()));
                        break;

                    default:
                        issues.Add(new DataIntegrityIssue
                        {
                            EntityType = "System",
                            EntityId = "Validation",
                            IssueType = "Invalid Request",
                            Description = $"Unknown entity type: {entityType}",
                            Severity = "Medium"
                        });
                        break;
                }

                Logger.Information("Entity validation completed for {EntityType} {EntityId}. Found {IssueCount} issues",
                    entityType, entityId, issues.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during entity validation for {EntityType} {EntityId}", entityType, entityId);
                issues.Add(new DataIntegrityIssue
                {
                    EntityType = entityType,
                    EntityId = entityId.ToString(),
                    IssueType = "Validation Error",
                    Description = $"Entity validation failed: {ex.Message}",
                    Severity = "Critical"
                });
            }

            return issues;
        }
    }
}
