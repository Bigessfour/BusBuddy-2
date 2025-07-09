using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Buddy.Migrations
{
    /// <inheritdoc />
    public partial class InitialEnhancedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_Drivers_DriverId",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_Routes_RouteId",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_Vehicles_VehicleId",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_Fuel_Vehicles_VehicleId",
                table: "Fuel");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenance_Vehicles_VehicleId",
                table: "Maintenance");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Drivers_AMDriverId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Drivers_PMDriverId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Vehicles_AMVehicleId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Vehicles_PMVehicleId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Fuel_VehicleId",
                table: "Fuel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Activity",
                table: "Activity");

            migrationBuilder.DropIndex(
                name: "IX_Activity_RouteId",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "MaintenanceType",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "FuelStation",
                table: "Fuel");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Fuel");

            migrationBuilder.DropColumn(
                name: "MilesPerGallon",
                table: "Fuel");

            migrationBuilder.DropColumn(
                name: "OdometerReading",
                table: "Fuel");

            migrationBuilder.DropColumn(
                name: "EndOdometer",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "StartOdometer",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Activity");

            migrationBuilder.RenameTable(
                name: "Activity",
                newName: "Activities");

            migrationBuilder.RenameColumn(
                name: "ShopVendor",
                table: "Maintenance",
                newName: "WorkOrderNumber");

            migrationBuilder.RenameColumn(
                name: "MaintenanceDate",
                table: "Maintenance",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "Maintenance",
                newName: "PartsCost");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Fuel",
                newName: "VehicleOdometerReading");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Activities",
                newName: "AssignedVehicleId");

            migrationBuilder.RenameColumn(
                name: "ActivityDate",
                table: "Activities",
                newName: "Date");

            migrationBuilder.RenameIndex(
                name: "IX_Activity_VehicleId",
                table: "Activities",
                newName: "IX_Activities_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_Activity_DriverId",
                table: "Activities",
                newName: "IX_Activities_DriverId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Vehicles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FleetType",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelCapacity",
                table: "Vehicles",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FuelType",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GPSDeviceId",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GPSTracking",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastServiceDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MilesPerGallon",
                table: "Vehicles",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMaintenanceDue",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextMaintenanceMileage",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Vehicles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialEquipment",
                table: "Vehicles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeContact",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativePhone",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorPhone",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DropoffAddress",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FieldTripPermission",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Students",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medications",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhotoPermission",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialAccommodations",
                table: "Students",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SpecialNeeds",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OdometerReading",
                table: "Maintenance",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Maintenance",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Maintenance",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Maintenance",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<decimal>(
                name: "LaborCost",
                table: "Maintenance",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborHours",
                table: "Maintenance",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceCompleted",
                table: "Maintenance",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartsUsed",
                table: "Maintenance",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Maintenance",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Normal");

            migrationBuilder.AddColumn<decimal>(
                name: "RepairCost",
                table: "Maintenance",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Maintenance",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Maintenance",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vendor",
                table: "Maintenance",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Warranty",
                table: "Maintenance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyExpiry",
                table: "Maintenance",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalCost",
                table: "Fuel",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerGallon",
                table: "Fuel",
                type: "decimal(8,3)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Gallons",
                table: "Fuel",
                type: "decimal(8,3)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,3)");

            migrationBuilder.AddColumn<string>(
                name: "FuelLocation",
                table: "Fuel",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FuelType",
                table: "Fuel",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Gasoline");

            migrationBuilder.AddColumn<int>(
                name: "VehicleFueledId",
                table: "Fuel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Drivers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Drivers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Drivers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "BackgroundCheckDate",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BackgroundCheckExpiry",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Drivers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Drivers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DrugTestDate",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DrugTestExpiry",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Drivers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Drivers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalRestrictions",
                table: "Drivers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Drivers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhysicalExamDate",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhysicalExamExpiry",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Drivers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityType",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "ActivityCategory",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualCost",
                table: "Activities",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Activities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovalRequired",
                table: "Activities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Activities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Activities",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "Activities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "Activities",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EventTime",
                table: "Activities",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "LeaveTime",
                table: "Activities",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "RequestedBy",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Activities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Scheduled");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Activities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Activities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Activities",
                table: "Activities",
                column: "ActivityId");

            migrationBuilder.CreateTable(
                name: "ActivitySchedule",
                columns: table => new
                {
                    ActivityScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TripType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScheduledVehicleId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDestination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ScheduledLeaveTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ScheduledEventTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ScheduledRiders = table.Column<int>(type: "int", nullable: true),
                    ScheduledDriverId = table.Column<int>(type: "int", nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySchedule", x => x.ActivityScheduleId);
                    table.ForeignKey(
                        name: "FK_ActivitySchedule_Driver",
                        column: x => x.ScheduledDriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivitySchedule_Vehicle",
                        column: x => x.ScheduledVehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteStops",
                columns: table => new
                {
                    RouteStopId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    StopName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StopAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    StopOrder = table.Column<int>(type: "int", nullable: false),
                    ScheduledArrival = table.Column<TimeSpan>(type: "time", nullable: false),
                    ScheduledDeparture = table.Column<TimeSpan>(type: "time", nullable: false),
                    StopDuration = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStops", x => x.RouteStopId);
                    table.ForeignKey(
                        name: "FK_RouteStops_Route",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedules_Bus",
                        column: x => x.BusId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedules_Driver",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedules_Route",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolCalendar",
                columns: table => new
                {
                    CalendarId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SchoolYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RoutesRequired = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCalendar", x => x.CalendarId);
                });

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 1,
                columns: new[] { "BackgroundCheckDate", "BackgroundCheckExpiry", "CreatedBy", "CreatedDate", "DrugTestDate", "DrugTestExpiry", "EmergencyContactName", "EmergencyContactPhone", "FirstName", "LastName", "MedicalRestrictions", "Notes", "PhysicalExamDate", "PhysicalExamExpiry", "UpdatedBy", "UpdatedDate" },
                values: new object[] { null, null, null, new DateTime(2025, 7, 3, 18, 31, 36, 106, DateTimeKind.Utc).AddTicks(1651), null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 2,
                columns: new[] { "BackgroundCheckDate", "BackgroundCheckExpiry", "CreatedBy", "CreatedDate", "DrugTestDate", "DrugTestExpiry", "EmergencyContactName", "EmergencyContactPhone", "FirstName", "LastName", "MedicalRestrictions", "Notes", "PhysicalExamDate", "PhysicalExamExpiry", "UpdatedBy", "UpdatedDate" },
                values: new object[] { null, null, null, new DateTime(2025, 7, 3, 18, 31, 36, 106, DateTimeKind.Utc).AddTicks(1660), null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                columns: new[] { "CreatedBy", "CreatedDate", "Department", "FleetType", "FuelCapacity", "FuelType", "GPSDeviceId", "GPSTracking", "LastServiceDate", "MilesPerGallon", "NextMaintenanceDue", "NextMaintenanceMileage", "Notes", "SpecialEquipment", "UpdatedBy", "UpdatedDate" },
                values: new object[] { null, new DateTime(2025, 7, 3, 18, 31, 36, 106, DateTimeKind.Utc).AddTicks(1559), null, null, null, null, null, false, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                columns: new[] { "CreatedBy", "CreatedDate", "Department", "FleetType", "FuelCapacity", "FuelType", "GPSDeviceId", "GPSTracking", "LastServiceDate", "MilesPerGallon", "NextMaintenanceDue", "NextMaintenanceMileage", "Notes", "SpecialEquipment", "UpdatedBy", "UpdatedDate" },
                values: new object[] { null, new DateTime(2025, 7, 3, 18, 31, 36, 106, DateTimeKind.Utc).AddTicks(1602), null, null, null, null, null, false, null, null, null, null, null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DateLastInspection",
                table: "Vehicles",
                column: "DateLastInspection");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_FleetType",
                table: "Vehicles",
                column: "FleetType");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_InsuranceExpiryDate",
                table: "Vehicles",
                column: "InsuranceExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicenseNumber",
                table: "Vehicles",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_MakeModelYear",
                table: "Vehicles",
                columns: new[] { "Make", "Model", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Status",
                table: "Vehicles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Active",
                table: "Students",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Grade",
                table: "Students",
                column: "Grade");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Name",
                table: "Students",
                column: "StudentName");

            migrationBuilder.CreateIndex(
                name: "IX_Students_School",
                table: "Students",
                column: "School");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_Date",
                table: "Routes",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DateRouteName",
                table: "Routes",
                columns: new[] { "Date", "RouteName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_RouteName",
                table: "Routes",
                column: "RouteName");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenance_Date",
                table: "Maintenance",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenance_Priority",
                table: "Maintenance",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenance_Type",
                table: "Maintenance",
                column: "MaintenanceCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenance_VehicleDate",
                table: "Maintenance",
                columns: new[] { "VehicleId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Fuel_FuelDate",
                table: "Fuel",
                column: "FuelDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fuel_Location",
                table: "Fuel",
                column: "FuelLocation");

            migrationBuilder.CreateIndex(
                name: "IX_Fuel_Type",
                table: "Fuel",
                column: "FuelType");

            migrationBuilder.CreateIndex(
                name: "IX_Fuel_VehicleDate",
                table: "Fuel",
                columns: new[] { "VehicleFueledId", "FuelDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Fuel_VehicleId",
                table: "Fuel",
                column: "VehicleFueledId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_Email",
                table: "Drivers",
                column: "DriverEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_LicenseExpiration",
                table: "Drivers",
                column: "LicenseExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_LicenseType",
                table: "Drivers",
                column: "DriversLicenceType");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_Phone",
                table: "Drivers",
                column: "DriverPhone");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TrainingComplete",
                table: "Drivers",
                column: "TrainingComplete");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ActivityType",
                table: "Activities",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ApprovalRequired",
                table: "Activities",
                column: "ApprovalRequired");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Date",
                table: "Activities",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_DateTimeRange",
                table: "Activities",
                columns: new[] { "Date", "LeaveTime", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_DriverSchedule",
                table: "Activities",
                columns: new[] { "DriverId", "Date", "LeaveTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Status",
                table: "Activities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_VehicleSchedule",
                table: "Activities",
                columns: new[] { "AssignedVehicleId", "Date", "LeaveTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedule_Date",
                table: "ActivitySchedule",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedule_DriverId",
                table: "ActivitySchedule",
                column: "ScheduledDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedule_TripType",
                table: "ActivitySchedule",
                column: "TripType");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedule_VehicleId",
                table: "ActivitySchedule",
                column: "ScheduledVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteId",
                table: "RouteStops",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteOrder",
                table: "RouteStops",
                columns: new[] { "RouteId", "StopOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_BusId",
                table: "Schedules",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_Date",
                table: "Schedules",
                column: "ScheduleDate");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DriverId",
                table: "Schedules",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_RouteId",
                table: "Schedules",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCalendar_Date",
                table: "SchoolCalendar",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCalendar_EventType",
                table: "SchoolCalendar",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCalendar_RoutesRequired",
                table: "SchoolCalendar",
                column: "RoutesRequired");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCalendar_SchoolYear",
                table: "SchoolCalendar",
                column: "SchoolYear");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Driver",
                table: "Activities",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Vehicle",
                table: "Activities",
                column: "AssignedVehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fuel_Vehicle",
                table: "Fuel",
                column: "VehicleFueledId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_Vehicle",
                table: "Maintenance",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_AMDriver",
                table: "Routes",
                column: "AMDriverId",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_AMVehicle",
                table: "Routes",
                column: "AMVehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_PMDriver",
                table: "Routes",
                column: "PMDriverId",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_PMVehicle",
                table: "Routes",
                column: "PMVehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Driver",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Vehicle",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Fuel_Vehicle",
                table: "Fuel");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenance_Vehicle",
                table: "Maintenance");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_AMDriver",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_AMVehicle",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_PMDriver",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_PMVehicle",
                table: "Routes");

            migrationBuilder.DropTable(
                name: "ActivitySchedule");

            migrationBuilder.DropTable(
                name: "RouteStops");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "SchoolCalendar");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_DateLastInspection",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_FleetType",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_InsuranceExpiryDate",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicenseNumber",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_MakeModelYear",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Status",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Students_Active",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_Grade",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_Name",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_School",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Routes_Date",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_DateRouteName",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_RouteName",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Maintenance_Date",
                table: "Maintenance");

            migrationBuilder.DropIndex(
                name: "IX_Maintenance_Priority",
                table: "Maintenance");

            migrationBuilder.DropIndex(
                name: "IX_Maintenance_Type",
                table: "Maintenance");

            migrationBuilder.DropIndex(
                name: "IX_Maintenance_VehicleDate",
                table: "Maintenance");

            migrationBuilder.DropIndex(
                name: "IX_Fuel_FuelDate",
                table: "Fuel");

            migrationBuilder.DropIndex(
                name: "IX_Fuel_Location",
                table: "Fuel");

            migrationBuilder.DropIndex(
                name: "IX_Fuel_Type",
                table: "Fuel");

            migrationBuilder.DropIndex(
                name: "IX_Fuel_VehicleDate",
                table: "Fuel");

            migrationBuilder.DropIndex(
                name: "IX_Fuel_VehicleId",
                table: "Fuel");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_Email",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_LicenseExpiration",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_LicenseType",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_Phone",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_TrainingComplete",
                table: "Drivers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Activities",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ActivityType",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ApprovalRequired",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_Date",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_DateTimeRange",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_DriverSchedule",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_Status",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_VehicleSchedule",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FleetType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FuelCapacity",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FuelType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "GPSDeviceId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "GPSTracking",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LastServiceDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "MilesPerGallon",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceDue",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceMileage",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "SpecialEquipment",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AlternativeContact",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AlternativePhone",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DoctorPhone",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DropoffAddress",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FieldTripPermission",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Medications",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PhotoPermission",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PickupAddress",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SpecialAccommodations",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SpecialNeeds",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "LaborCost",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "LaborHours",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "MaintenanceCompleted",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "PartsUsed",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "RepairCost",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "Vendor",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "Warranty",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "WarrantyExpiry",
                table: "Maintenance");

            migrationBuilder.DropColumn(
                name: "FuelLocation",
                table: "Fuel");

            migrationBuilder.DropColumn(
                name: "FuelType",
                table: "Fuel");

            migrationBuilder.DropColumn(
                name: "VehicleFueledId",
                table: "Fuel");

            migrationBuilder.DropColumn(
                name: "BackgroundCheckDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "BackgroundCheckExpiry",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DrugTestDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DrugTestExpiry",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "MedicalRestrictions",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "PhysicalExamDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "PhysicalExamExpiry",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "ActivityCategory",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ActualCost",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ApprovalRequired",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Destination",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EventTime",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "LeaveTime",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RequestedBy",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Activities");

            migrationBuilder.RenameTable(
                name: "Activities",
                newName: "Activity");

            migrationBuilder.RenameColumn(
                name: "WorkOrderNumber",
                table: "Maintenance",
                newName: "ShopVendor");

            migrationBuilder.RenameColumn(
                name: "PartsCost",
                table: "Maintenance",
                newName: "Cost");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Maintenance",
                newName: "MaintenanceDate");

            migrationBuilder.RenameColumn(
                name: "VehicleOdometerReading",
                table: "Fuel",
                newName: "VehicleId");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Activity",
                newName: "ActivityDate");

            migrationBuilder.RenameColumn(
                name: "AssignedVehicleId",
                table: "Activity",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_Activities_VehicleId",
                table: "Activity",
                newName: "IX_Activity_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_Activities_DriverId",
                table: "Activity",
                newName: "IX_Activity_DriverId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<int>(
                name: "OdometerReading",
                table: "Maintenance",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Maintenance",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceType",
                table: "Maintenance",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalCost",
                table: "Fuel",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerGallon",
                table: "Fuel",
                type: "decimal(8,3)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Gallons",
                table: "Fuel",
                type: "decimal(8,3)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,3)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FuelStation",
                table: "Fuel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Fuel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MilesPerGallon",
                table: "Fuel",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OdometerReading",
                table: "Fuel",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Drivers",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Drivers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Drivers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityType",
                table: "Activity",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "EndOdometer",
                table: "Activity",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Activity",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RouteId",
                table: "Activity",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartOdometer",
                table: "Activity",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Activity",
                type: "time",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Activity",
                table: "Activity",
                column: "ActivityId");

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 1,
                columns: new[] { "FirstName", "LastName" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 2,
                columns: new[] { "FirstName", "LastName" },
                values: new object[] { "", "" });

            migrationBuilder.CreateIndex(
                name: "IX_Fuel_VehicleId",
                table: "Fuel",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_RouteId",
                table: "Activity",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_Drivers_DriverId",
                table: "Activity",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_Routes_RouteId",
                table: "Activity",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_Vehicles_VehicleId",
                table: "Activity",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fuel_Vehicles_VehicleId",
                table: "Fuel",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_Vehicles_VehicleId",
                table: "Maintenance",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Drivers_AMDriverId",
                table: "Routes",
                column: "AMDriverId",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Drivers_PMDriverId",
                table: "Routes",
                column: "PMDriverId",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Vehicles_AMVehicleId",
                table: "Routes",
                column: "AMVehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Vehicles_PMVehicleId",
                table: "Routes",
                column: "PMVehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
