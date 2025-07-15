using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBuddy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogTimestampIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Timestamp",
                table: "ActivityLogs",
                column: "Timestamp",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_Timestamp",
                table: "ActivityLogs");

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverID",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 12, 8, 46, 17, 845, DateTimeKind.Utc).AddTicks(5335));

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverID",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 12, 8, 46, 17, 845, DateTimeKind.Utc).AddTicks(5347));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 12, 8, 46, 17, 845, DateTimeKind.Utc).AddTicks(5084));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 12, 8, 46, 17, 845, DateTimeKind.Utc).AddTicks(5131));
        }
    }
}
