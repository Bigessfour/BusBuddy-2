using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Buddy.Migrations
{
    /// <inheritdoc />
    public partial class MakeDriverIdOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "AMBeginTime",
                table: "Routes",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusNumber",
                table: "Routes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Distance",
                table: "Routes",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "Routes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDuration",
                table: "Routes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PMBeginTime",
                table: "Routes",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StopCount",
                table: "Routes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentCount",
                table: "Routes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "Activities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "DestinationLatitude",
                table: "Activities",
                type: "decimal(10,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DestinationLongitude",
                table: "Activities",
                type: "decimal(11,8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Directions",
                table: "Activities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DistanceMiles",
                table: "Activities",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedTravelTime",
                table: "Activities",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PickupLatitude",
                table: "Activities",
                type: "decimal(10,8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupLocation",
                table: "Activities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PickupLongitude",
                table: "Activities",
                type: "decimal(11,8)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AMBeginTime",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "BusNumber",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "PMBeginTime",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "StopCount",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "StudentCount",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Directions",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "DistanceMiles",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EstimatedTravelTime",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PickupLatitude",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PickupLocation",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "PickupLongitude",
                table: "Activities");

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 3, 23, 46, 42, 641, DateTimeKind.Utc).AddTicks(7208));

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 3, 23, 46, 42, 641, DateTimeKind.Utc).AddTicks(7216));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 3, 23, 46, 42, 641, DateTimeKind.Utc).AddTicks(7107));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 3, 23, 46, 42, 641, DateTimeKind.Utc).AddTicks(7155));
        }
    }
}
