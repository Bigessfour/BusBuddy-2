using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BusBuddy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    User = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    DestinationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DestinationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaxCapacity = table.Column<int>(type: "INTEGER", nullable: true),
                    SpecialRequirements = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.DestinationId);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    DriverId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DriverName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DriverPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DriverEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Zip = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DriversLicenceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TrainingComplete = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Active"),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LicenseNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LicenseClass = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    LicenseIssueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Endorsements = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    MedicalRestrictions = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BackgroundCheckDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BackgroundCheckExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DrugTestDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DrugTestExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PhysicalExamDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PhysicalExamExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.DriverId);
                });

            migrationBuilder.CreateTable(
                name: "Fuel",
                columns: table => new
                {
                    FuelId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FuelDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FuelLocation = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    VehicleFueledId = table.Column<int>(type: "INTEGER", nullable: false),
                    VehicleOdometerReading = table.Column<int>(type: "INTEGER", nullable: false),
                    FuelType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Gallons = table.Column<decimal>(type: "decimal(8,3)", nullable: true),
                    PricePerGallon = table.Column<decimal>(type: "decimal(8,3)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fuel", x => x.FuelId);
                });

            migrationBuilder.CreateTable(
                name: "Maintenance",
                columns: table => new
                {
                    MaintenanceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    OdometerReading = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintenanceCompleted = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RepairCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PerformedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NextServiceDue = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextServiceOdometer = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    WorkOrderNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Warranty = table.Column<bool>(type: "INTEGER", nullable: false),
                    WarrantyExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PartsUsed = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    LaborHours = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PartsCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance", x => x.MaintenanceId);
                });

            migrationBuilder.CreateTable(
                name: "RouteStops",
                columns: table => new
                {
                    RouteStopId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RouteId = table.Column<int>(type: "INTEGER", nullable: false),
                    StopName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StopAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    StopOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledArrival = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ScheduledDeparture = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    StopDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStops", x => x.RouteStopId);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StudentNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Grade = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    School = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HomeAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Zip = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    HomePhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ParentGuardian = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EmergencyPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    MedicalNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TransportationNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AMRoute = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PMRoute = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BusStop = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PickupAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DropoffAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SpecialNeeds = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpecialAccommodations = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Allergies = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Medications = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DoctorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DoctorPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    AlternativeContact = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AlternativePhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PhotoPermission = table.Column<bool>(type: "INTEGER", nullable: false),
                    FieldTripPermission = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Make = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PlateNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActivityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Destination = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DestinationId = table.Column<int>(type: "INTEGER", nullable: true),
                    DestinationOverride = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LeaveTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EventTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    RequestedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AssignedVehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    DriverId = table.Column<int>(type: "INTEGER", nullable: true),
                    StudentsCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RouteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ReturnTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ExpectedPassengers = table.Column<int>(type: "INTEGER", nullable: true),
                    RecurringSeriesId = table.Column<int>(type: "INTEGER", nullable: true),
                    ActivityCategory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ActualCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ApprovalRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Approved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DestinationLatitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    DestinationLongitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    DistanceMiles = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    EstimatedTravelTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Directions = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PickupLocation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PickupLatitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    PickupLongitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_Activities_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "DestinationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Activities_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId");
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    RouteId = table.Column<int>(type: "INTEGER", nullable: false),
                    DriverId = table.Column<int>(type: "INTEGER", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SportsCategory = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Opponent = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DestinationTown = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DepartTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    ScheduledTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedules_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TripEvents",
                columns: table => new
                {
                    TripEventId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    POCName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    POCPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    POCEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LeaveTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReturnTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: true),
                    DriverId = table.Column<int>(type: "INTEGER", nullable: true),
                    RouteId = table.Column<int>(type: "INTEGER", nullable: true),
                    StudentCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AdultSupervisorCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Destination = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SpecialRequirements = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TripNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ApprovalRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripEvents", x => x.TripEventId);
                    table.ForeignKey(
                        name: "FK_TripEvents_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId");
                });

            migrationBuilder.CreateTable(
                name: "ActivitySchedule",
                columns: table => new
                {
                    ActivityScheduleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduledDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TripType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ScheduledVehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledDestination = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ScheduledLeaveTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ScheduledEventTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ScheduledRiders = table.Column<int>(type: "INTEGER", nullable: true),
                    ScheduledDriverId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TripEventId = table.Column<int>(type: "INTEGER", nullable: true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySchedule", x => x.ActivityScheduleId);
                    table.ForeignKey(
                        name: "FK_ActivitySchedule_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "ActivityId");
                    table.ForeignKey(
                        name: "FK_ActivitySchedule_Drivers_ScheduledDriverId",
                        column: x => x.ScheduledDriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivitySchedule_TripEvents_TripEventId",
                        column: x => x.TripEventId,
                        principalTable: "TripEvents",
                        principalColumn: "TripEventId");
                });

            migrationBuilder.CreateTable(
                name: "StudentSchedules",
                columns: table => new
                {
                    StudentScheduleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: true),
                    ActivityScheduleId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignmentType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PickupLocation = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DropoffLocation = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Confirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Attended = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSchedules", x => x.StudentScheduleId);
                    table.ForeignKey(
                        name: "FK_StudentSchedules_ActivitySchedule_ActivityScheduleId",
                        column: x => x.ActivityScheduleId,
                        principalTable: "ActivitySchedule",
                        principalColumn: "ActivityScheduleId");
                    table.ForeignKey(
                        name: "FK_StudentSchedules_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "ScheduleId");
                    table.ForeignKey(
                        name: "FK_StudentSchedules_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Destinations",
                columns: new[] { "DestinationId", "Address", "City", "ContactEmail", "ContactName", "ContactPhone", "CreatedBy", "CreatedDate", "DestinationType", "IsActive", "IsDeleted", "Latitude", "Longitude", "MaxCapacity", "Name", "SpecialRequirements", "State", "UpdatedBy", "UpdatedDate", "ZipCode" },
                values: new object[,]
                {
                    { 1, "200 Central Park West", "New York", "education@amnh.org", "Education Director", "212-769-5100", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Field Trip", true, false, 40.7813m, -73.9740m, 200, "Natural History Museum", null, "NY", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(2979), "10024" },
                    { 2, "1234 Education Blvd", "Springfield", "athletics@centralhs.edu", "Athletic Director", "217-555-0200", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sports Event", true, false, 39.7817m, -89.6501m, 500, "Central High School", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4406), "62701" },
                    { 3, "456 University Ave", "Champaign", "competitions@university.edu", "Competition Coordinator", "217-555-0300", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Academic Competition", true, false, 40.1020m, -88.2272m, 150, "University Campus Science Building", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4411), "61820" },
                    { 4, "789 Discovery Lane", "Peoria", "programs@sciencecenter.org", "Program Manager", "309-555-0400", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Field Trip", true, false, 40.6936m, -89.5890m, 180, "Regional Science Center", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4423), "61602" },
                    { 5, "321 Arts Plaza", "Bloomington", "events@pacenter.org", "Event Coordinator", "309-555-0500", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Band Competition", true, false, 40.4842m, -88.9937m, 300, "Performing Arts Center", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4426), "61701" },
                    { 6, "654 Sports Complex Dr", "Normal", "manager@regionalstadium.com", "Stadium Manager", "309-555-0600", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sports Event", true, false, 40.5142m, -88.9906m, 1000, "Regional Stadium", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4429), "61761" },
                    { 7, "987 Volunteer Way", "Decatur", "volunteers@foodbank.org", "Volunteer Coordinator", "217-555-0700", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Community Service", true, false, 39.8403m, -88.9548m, 50, "Community Food Bank", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4432), "62521" },
                    { 8, "147 Culture Street", "Chicago", "education@metart.org", "Education Specialist", "312-555-0800", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Field Trip", true, false, 41.8781m, -87.6298m, 250, "Metropolitan Art Museum", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4435), "60601" },
                    { 9, "258 Conference Blvd", "Rockford", "services@conventioncenter.com", "Event Services", "815-555-0900", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Academic Competition", true, false, 42.2711m, -89.0940m, 400, "Convention Center East", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4452), "61101" },
                    { 10, "369 Pool Lane", "Carbondale", "manager@aquaticcenter.com", "Facility Manager", "618-555-1000", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sports Event", true, false, 37.7273m, -89.2167m, 200, "Aquatic Center", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4455), "62901" },
                    { 11, "741 Galaxy Road", "Aurora", "education@planetarium.org", "Education Director", "630-555-1100", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Field Trip", true, false, 41.7606m, -88.3201m, 120, "Planetarium & Space Center", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4458), "60506" },
                    { 12, "852 Stage Avenue", "Joliet", "manager@communitytheatre.org", "Theater Manager", "815-555-1200", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Drama Performance", true, false, 41.5250m, -88.0817m, 180, "Community Theater", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4461), "60431" },
                    { 13, "963 Conservation Trail", "Champaign", "ranger@prairiepark.gov", "Park Ranger", "217-555-1300", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Field Trip", true, false, 40.1164m, -88.2434m, 80, "Prairie Nature Reserve", "Weather-dependent, hiking boots recommended", "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4464), "61822" },
                    { 14, "159 Enterprise Parkway", "Schaumburg", "careers@bizdistrict.com", "Career Coordinator", "847-555-1400", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Career Fair", true, false, 42.0334m, -88.0834m, 300, "Business Innovation District", null, "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4540), "60173" },
                    { 15, "753 Graduation Circle", "Springfield", "events@memorialaud.gov", "Event Coordinator", "217-555-1500", "System", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Graduation Ceremony", true, false, 39.7990m, -89.6439m, 800, "Memorial Auditorium", "Formal dress code, restricted parking", "IL", null, new DateTime(2025, 7, 26, 0, 1, 41, 858, DateTimeKind.Utc).AddTicks(4543), "62703" }
                });

            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "DriverId", "Address", "BackgroundCheckDate", "BackgroundCheckExpiry", "City", "CreatedBy", "CreatedDate", "DriverEmail", "DriverName", "DriverPhone", "DriversLicenceType", "DrugTestDate", "DrugTestExpiry", "EmergencyContactName", "EmergencyContactPhone", "Endorsements", "FirstName", "HireDate", "LastName", "LicenseClass", "LicenseExpiryDate", "LicenseIssueDate", "LicenseNumber", "MedicalRestrictions", "Notes", "PhysicalExamDate", "PhysicalExamExpiry", "State", "Status", "TrainingComplete", "UpdatedBy", "UpdatedDate", "Zip" },
                values: new object[,]
                {
                    { 1, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "m.rodriguez@busbuddy.edu", "Michael Rodriguez", "555-0123", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 2, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "s.johnson@busbuddy.edu", "Sarah Johnson", "555-0124", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 3, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "d.chen@busbuddy.edu", "David Chen", "555-0125", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 4, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "m.garcia@busbuddy.edu", "Maria Garcia", "555-0126", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 5, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "j.wilson@busbuddy.edu", "James Wilson", "555-0127", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 6, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "a.brown@busbuddy.edu", "Ashley Brown", "555-0128", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 7, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "r.martinez@busbuddy.edu", "Robert Martinez", "555-0129", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 8, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "j.lee@busbuddy.edu", "Jennifer Lee", "555-0130", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 9, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "c.davis@busbuddy.edu", "Christopher Davis", "555-0131", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 10, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "a.thompson@busbuddy.edu", "Amanda Thompson", "555-0132", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 11, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "k.anderson@busbuddy.edu", "Kevin Anderson", "555-0133", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 12, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "l.white@busbuddy.edu", "Lisa White", "555-0134", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 13, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "d.moore@busbuddy.edu", "Daniel Moore", "555-0135", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 14, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "n.taylor@busbuddy.edu", "Nicole Taylor", "555-0136", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 15, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "b.jackson@busbuddy.edu", "Brandon Jackson", "555-0137", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 16, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "s.miller@busbuddy.edu", "Stephanie Miller", "555-0138", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 17, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "a.harris@busbuddy.edu", "Andrew Harris", "555-0139", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 18, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "m.clark@busbuddy.edu", "Michelle Clark", "555-0140", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 19, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "r.lewis@busbuddy.edu", "Ryan Lewis", "555-0141", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null },
                    { 20, null, null, null, null, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "s.walker@busbuddy.edu", "Samantha Walker", "555-0142", "CDL Class B", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "Active", true, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Capacity", "Make", "Model", "PlateNumber" },
                values: new object[,]
                {
                    { 1, 72, "Blue Bird", "Vision", "SCH-001" },
                    { 2, 77, "Thomas Built", "C2", "SCH-002" },
                    { 3, 71, "IC Bus", "CE200", "SCH-003" },
                    { 4, 84, "Blue Bird", "All American RE", "SCH-004" },
                    { 5, 90, "Thomas Built", "Saf-T-Liner HDX", "SCH-005" },
                    { 6, 77, "IC Bus", "RE200", "SCH-006" },
                    { 7, 72, "Blue Bird", "Vision", "SCH-007" },
                    { 8, 77, "Thomas Built", "C2", "SCH-008" },
                    { 9, 71, "IC Bus", "CE200", "SCH-009" },
                    { 10, 48, "Blue Bird", "All American FE", "SCH-010" },
                    { 11, 24, "Thomas Built", "Minotour", "SCH-011" },
                    { 12, 90, "IC Bus", "AC200", "SCH-012" },
                    { 13, 30, "Blue Bird", "Micro Bird", "SCH-013" },
                    { 14, 35, "Thomas Built", "EFX", "SCH-014" },
                    { 15, 77, "IC Bus", "TE200", "SCH-015" }
                });

            migrationBuilder.InsertData(
                table: "Activities",
                columns: new[] { "ActivityId", "ActivityCategory", "ActivityType", "ActualCost", "ApprovalDate", "ApprovalRequired", "Approved", "ApprovedBy", "AssignedVehicleId", "CreatedBy", "CreatedDate", "Date", "Description", "Destination", "DestinationId", "DestinationLatitude", "DestinationLongitude", "DestinationOverride", "Directions", "DistanceMiles", "DriverId", "EstimatedCost", "EstimatedTravelTime", "EventTime", "ExpectedPassengers", "LeaveTime", "Notes", "PickupLatitude", "PickupLocation", "PickupLongitude", "RecurringSeriesId", "RequestedBy", "ReturnTime", "RouteId", "Status", "StudentsCount", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, null, "Field Trip", null, null, true, false, null, 1, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Natural History Museum", null, null, null, null, null, null, 1, null, null, new TimeSpan(0, 10, 0, 0, 0), null, new TimeSpan(0, 8, 30, 0, 0), null, null, "School", null, null, "Mrs. Thompson", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 45, null, null },
                    { 2, null, "Sports Event", null, null, true, false, null, 2, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Central High School", null, null, null, null, null, null, 2, null, null, new TimeSpan(0, 15, 30, 0, 0), null, new TimeSpan(0, 14, 0, 0, 0), null, null, "School", null, null, "Coach Martinez", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 22, null, null },
                    { 3, null, "Academic Competition", null, null, true, false, null, 3, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "University Campus", null, null, null, null, null, null, 3, null, null, new TimeSpan(0, 9, 0, 0, 0), null, new TimeSpan(0, 7, 45, 0, 0), null, null, "School", null, null, "Mr. Chen", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 15, null, null },
                    { 4, null, "Field Trip", null, null, true, false, null, 4, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Science Center", null, null, null, null, null, null, 4, null, null, new TimeSpan(0, 10, 30, 0, 0), null, new TimeSpan(0, 9, 15, 0, 0), null, null, "School", null, null, "Ms. Garcia", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 38, null, null },
                    { 5, null, "Band Competition", null, null, true, false, null, 5, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Performing Arts Center", null, null, null, null, null, null, 5, null, null, new TimeSpan(0, 14, 0, 0, 0), null, new TimeSpan(0, 12, 0, 0, 0), null, null, "School", null, null, "Mr. Wilson", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 65, null, null },
                    { 6, null, "Sports Event", null, null, true, false, null, 6, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Regional Stadium", null, null, null, null, null, null, 6, null, null, new TimeSpan(0, 18, 0, 0, 0), null, new TimeSpan(0, 16, 30, 0, 0), null, null, "School", null, null, "Coach Brown", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 28, null, null },
                    { 7, null, "Community Service", null, null, true, false, null, 7, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Local Food Bank", null, null, null, null, null, null, 7, null, null, new TimeSpan(0, 9, 0, 0, 0), null, new TimeSpan(0, 8, 0, 0, 0), null, null, "School", null, null, "Mrs. Martinez", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 20, null, null },
                    { 8, null, "Field Trip", null, null, true, false, null, 8, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Art Museum", null, null, null, null, null, null, 8, null, null, new TimeSpan(0, 12, 0, 0, 0), null, new TimeSpan(0, 10, 45, 0, 0), null, null, "School", null, null, "Ms. Lee", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 32, null, null },
                    { 9, null, "Academic Competition", null, null, true, false, null, 9, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Convention Center", null, null, null, null, null, null, 9, null, null, new TimeSpan(0, 8, 45, 0, 0), null, new TimeSpan(0, 7, 30, 0, 0), null, null, "School", null, null, "Mr. Davis", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 12, null, null },
                    { 10, null, "Sports Event", null, null, true, false, null, 10, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Aquatic Center", null, null, null, null, null, null, 10, null, null, new TimeSpan(0, 15, 0, 0, 0), null, new TimeSpan(0, 13, 15, 0, 0), null, null, "School", null, null, "Coach Thompson", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 18, null, null },
                    { 11, null, "Field Trip", null, null, true, false, null, 11, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Planetarium", null, null, null, null, null, null, 11, null, null, new TimeSpan(0, 11, 0, 0, 0), null, new TimeSpan(0, 9, 30, 0, 0), null, null, "School", null, null, "Dr. Anderson", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 25, null, null },
                    { 12, null, "Drama Performance", null, null, true, false, null, 12, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Community Theater", null, null, null, null, null, null, 12, null, null, new TimeSpan(0, 19, 0, 0, 0), null, new TimeSpan(0, 17, 0, 0, 0), null, null, "School", null, null, "Ms. White", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 35, null, null },
                    { 13, null, "Environmental Study", null, null, true, false, null, 13, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Nature Reserve", null, null, null, null, null, null, 13, null, null, new TimeSpan(0, 9, 30, 0, 0), null, new TimeSpan(0, 8, 15, 0, 0), null, null, "School", null, null, "Mr. Moore", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 28, null, null },
                    { 14, null, "Career Fair", null, null, true, false, null, 14, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Business District", null, null, null, null, null, null, 14, null, null, new TimeSpan(0, 13, 0, 0, 0), null, new TimeSpan(0, 11, 30, 0, 0), null, null, "School", null, null, "Ms. Taylor", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 42, null, null },
                    { 15, null, "Sports Event", null, null, true, false, null, 15, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Golf Course", null, null, null, null, null, null, 15, null, null, new TimeSpan(0, 8, 0, 0, 0), null, new TimeSpan(0, 6, 45, 0, 0), null, null, "School", null, null, "Coach Jackson", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 8, null, null },
                    { 16, null, "Field Trip", null, null, true, false, null, 1, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Historical Society", null, null, null, null, null, null, 16, null, null, new TimeSpan(0, 11, 30, 0, 0), null, new TimeSpan(0, 10, 0, 0, 0), null, null, "School", null, null, "Mrs. Miller", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 30, null, null },
                    { 17, null, "Academic Competition", null, null, true, false, null, 2, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Tech Campus", null, null, null, null, null, null, 17, null, null, new TimeSpan(0, 8, 30, 0, 0), null, new TimeSpan(0, 7, 15, 0, 0), null, null, "School", null, null, "Mr. Harris", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 16, null, null },
                    { 18, null, "Community Event", null, null, true, false, null, 3, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "City Hall", null, null, null, null, null, null, 18, null, null, new TimeSpan(0, 16, 0, 0, 0), null, new TimeSpan(0, 14, 30, 0, 0), null, null, "School", null, null, "Ms. Clark", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 50, null, null },
                    { 19, null, "Sports Event", null, null, true, false, null, 4, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Tennis Complex", null, null, null, null, null, null, 19, null, null, new TimeSpan(0, 17, 15, 0, 0), null, new TimeSpan(0, 15, 45, 0, 0), null, null, "School", null, null, "Coach Lewis", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 12, null, null },
                    { 20, null, "Field Trip", null, null, true, false, null, 5, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Botanical Gardens", null, null, null, null, null, null, 20, null, null, new TimeSpan(0, 10, 15, 0, 0), null, new TimeSpan(0, 9, 0, 0, 0), null, null, "School", null, null, "Mrs. Walker", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 40, null, null },
                    { 21, null, "Music Festival", null, null, true, false, null, 6, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Concert Hall", null, null, null, null, null, null, 1, null, null, new TimeSpan(0, 15, 30, 0, 0), null, new TimeSpan(0, 13, 30, 0, 0), null, null, "School", null, null, "Mr. Rodriguez", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 55, null, null },
                    { 22, null, "Science Fair", null, null, true, false, null, 7, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Exhibition Center", null, null, null, null, null, null, 2, null, null, new TimeSpan(0, 10, 0, 0, 0), null, new TimeSpan(0, 8, 45, 0, 0), null, null, "School", null, null, "Dr. Johnson", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 33, null, null },
                    { 23, null, "Sports Event", null, null, true, false, null, 8, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Basketball Arena", null, null, null, null, null, null, 3, null, null, new TimeSpan(0, 19, 0, 0, 0), null, new TimeSpan(0, 17, 15, 0, 0), null, null, "School", null, null, "Coach Chen", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 24, null, null },
                    { 24, null, "Cultural Exchange", null, null, true, false, null, 9, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "International Center", null, null, null, null, null, null, 4, null, null, new TimeSpan(0, 12, 0, 0, 0), null, new TimeSpan(0, 10, 30, 0, 0), null, null, "School", null, null, "Ms. Garcia", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 38, null, null },
                    { 25, null, "Field Trip", null, null, true, false, null, 10, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Space Center", null, null, null, null, null, null, 5, null, null, new TimeSpan(0, 9, 0, 0, 0), null, new TimeSpan(0, 7, 0, 0, 0), null, null, "School", null, null, "Mr. Wilson", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 48, null, null },
                    { 26, null, "Volunteer Work", null, null, true, false, null, 11, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Senior Center", null, null, null, null, null, null, 6, null, null, new TimeSpan(0, 15, 30, 0, 0), null, new TimeSpan(0, 14, 0, 0, 0), null, null, "School", null, null, "Mrs. Brown", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 22, null, null },
                    { 27, null, "Academic Competition", null, null, true, false, null, 12, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Library", null, null, null, null, null, null, 7, null, null, new TimeSpan(0, 11, 15, 0, 0), null, new TimeSpan(0, 9, 45, 0, 0), null, null, "School", null, null, "Mr. Martinez", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 14, null, null },
                    { 28, null, "Sports Event", null, null, true, false, null, 13, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Soccer Complex", null, null, null, null, null, null, 8, null, null, new TimeSpan(0, 18, 30, 0, 0), null, new TimeSpan(0, 16, 0, 0, 0), null, null, "School", null, null, "Coach Lee", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 26, null, null },
                    { 29, null, "Field Trip", null, null, true, false, null, 14, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Observatory", null, null, null, null, null, null, 9, null, null, new TimeSpan(0, 21, 0, 0, 0), null, new TimeSpan(0, 19, 30, 0, 0), null, null, "School", null, null, "Dr. Davis", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 20, null, null },
                    { 30, null, "Graduation Ceremony", null, null, true, false, null, 15, null, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Auditorium", null, null, null, null, null, null, 10, null, null, new TimeSpan(0, 19, 0, 0, 0), null, new TimeSpan(0, 17, 30, 0, 0), null, null, "School", null, null, "Principal Thompson", new TimeSpan(0, 0, 0, 0, 0), null, "Scheduled", 75, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_DestinationId",
                table: "Activities",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_DriverId",
                table: "Activities",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedule_ActivityId",
                table: "ActivitySchedule",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedule_ScheduledDriverId",
                table: "ActivitySchedule",
                column: "ScheduledDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedule_TripEventId",
                table: "ActivitySchedule",
                column: "TripEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_IsActive",
                table: "Destinations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_Name",
                table: "Destinations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_Type",
                table: "Destinations",
                column: "DestinationType");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DriverId",
                table: "Schedules",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSchedules_ActivityScheduleId",
                table: "StudentSchedules",
                column: "ActivityScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSchedules_ScheduleId",
                table: "StudentSchedules",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSchedules_StudentId",
                table: "StudentSchedules",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TripEvents_DriverId",
                table: "TripEvents",
                column: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Fuel");

            migrationBuilder.DropTable(
                name: "Maintenance");

            migrationBuilder.DropTable(
                name: "RouteStops");

            migrationBuilder.DropTable(
                name: "StudentSchedules");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "ActivitySchedule");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "TripEvents");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "Drivers");
        }
    }
}
