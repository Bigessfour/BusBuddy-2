using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Buddy.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteIdToActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RouteId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    TravelDate = table.Column<DateTime>(type: "date", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TicketType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Valid"),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QRCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRefundable = table.Column<bool>(type: "bit", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    UsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedByDriver = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_Tickets_Route",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Student",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Activities_RouteId",
                table: "Activities",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_IssuedDate",
                table: "Tickets",
                column: "IssuedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_QRCode",
                table: "Tickets",
                column: "QRCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_RouteId",
                table: "Tickets",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status",
                table: "Tickets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_StudentId",
                table: "Tickets",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_StudentRouteDate",
                table: "Tickets",
                columns: new[] { "StudentId", "RouteId", "TravelDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketType",
                table: "Tickets",
                column: "TicketType");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TravelDate",
                table: "Tickets",
                column: "TravelDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TravelDateStatus",
                table: "Tickets",
                columns: new[] { "TravelDate", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Route",
                table: "Activities",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Route",
                table: "Activities");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Activities_RouteId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "Activities");

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 3, 18, 31, 36, 106, DateTimeKind.Utc).AddTicks(1651));

            migrationBuilder.UpdateData(
                table: "Drivers",
                keyColumn: "DriverId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 3, 18, 31, 36, 106, DateTimeKind.Utc).AddTicks(1660));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 3, 18, 31, 36, 106, DateTimeKind.Utc).AddTicks(1559));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "VehicleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 3, 18, 31, 36, 106, DateTimeKind.Utc).AddTicks(1602));
        }
    }
}
