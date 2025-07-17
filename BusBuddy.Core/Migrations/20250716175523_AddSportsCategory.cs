using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBuddy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddSportsCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DepartTime",
                table: "Schedules",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationTown",
                table: "Schedules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Schedules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent",
                table: "Schedules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ScheduledTime",
                table: "Schedules",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SportsCategory",
                table: "Schedules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "ActivitySchedule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentSchedules",
                columns: table => new
                {
                    StudentScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    ActivityScheduleId = table.Column<int>(type: "int", nullable: true),
                    AssignmentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: ""),
                    PickupLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DropoffLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    Attended = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSchedules", x => x.StudentScheduleId);
                    table.ForeignKey(
                        name: "FK_StudentSchedules_ActivitySchedule",
                        column: x => x.ActivityScheduleId,
                        principalTable: "ActivitySchedule",
                        principalColumn: "ActivityScheduleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSchedules_Schedule",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "ScheduleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSchedules_Student",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverID",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 16, 17, 55, 22, 321, DateTimeKind.Utc).AddTicks(6764));

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverID",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 16, 17, 55, 22, 322, DateTimeKind.Utc).AddTicks(1053));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 16, 17, 55, 22, 320, DateTimeKind.Utc).AddTicks(1618));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 16, 17, 55, 22, 321, DateTimeKind.Utc).AddTicks(1060));

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedule_ActivityId",
                table: "ActivitySchedule",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSchedules_ActivityScheduleId",
                table: "StudentSchedules",
                column: "ActivityScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSchedules_AssignmentType",
                table: "StudentSchedules",
                column: "AssignmentType");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSchedules_ScheduleId",
                table: "StudentSchedules",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSchedules_StudentId",
                table: "StudentSchedules",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSchedules_StudentSchedule",
                table: "StudentSchedules",
                columns: new[] { "StudentId", "ScheduleId" },
                unique: true,
                filter: "[ScheduleId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivitySchedule_Activities_ActivityId",
                table: "ActivitySchedule",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "ActivityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivitySchedule_Activities_ActivityId",
                table: "ActivitySchedule");

            migrationBuilder.DropTable(
                name: "StudentSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ActivitySchedule_ActivityId",
                table: "ActivitySchedule");

            migrationBuilder.DropColumn(
                name: "DepartTime",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "DestinationTown",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Opponent",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "ScheduledTime",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "SportsCategory",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "ActivitySchedule");

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverID",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 15, 10, 31, 40, 969, DateTimeKind.Utc).AddTicks(5864));

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverID",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 15, 10, 31, 40, 969, DateTimeKind.Utc).AddTicks(5878));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 15, 10, 31, 40, 969, DateTimeKind.Utc).AddTicks(5562));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 15, 10, 31, 40, 969, DateTimeKind.Utc).AddTicks(5607));
        }
    }
}
