using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Buddy.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    User = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 8, 23, 37, 55, 599, DateTimeKind.Utc).AddTicks(6046));

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 8, 23, 37, 55, 599, DateTimeKind.Utc).AddTicks(6056));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 8, 23, 37, 55, 599, DateTimeKind.Utc).AddTicks(5868));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 8, 23, 37, 55, 599, DateTimeKind.Utc).AddTicks(5904));

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_RouteBusDeparture",
                table: "Schedules",
                columns: new[] { "RouteId", "BusId", "DepartureTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_RouteBusDeparture",
                table: "Schedules");

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 5, 22, 29, 38, 103, DateTimeKind.Utc).AddTicks(7895));

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 5, 22, 29, 38, 103, DateTimeKind.Utc).AddTicks(7907));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 5, 22, 29, 38, 103, DateTimeKind.Utc).AddTicks(7792));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 5, 22, 29, 38, 103, DateTimeKind.Utc).AddTicks(7835));
        }
    }
}
