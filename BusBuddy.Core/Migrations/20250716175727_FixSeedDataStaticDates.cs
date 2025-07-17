using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBuddy.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataStaticDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverID",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverID",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
