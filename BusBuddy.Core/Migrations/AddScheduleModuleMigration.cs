using Microsoft.EntityFrameworkCore.Migrations;

namespace BusBuddy.Core.Migrations
{
    public partial class AddScheduleModuleMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(nullable: false),
                    BusId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: false),
                    DepartureTime = table.Column<DateTime>(nullable: false),
                    ArrivalTime = table.Column<DateTime>(nullable: false),
                    ScheduleDate = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleId);
                    table.ForeignKey("FK_Schedules_Route", x => x.RouteId, "Routes", "RouteId", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Schedules_Bus", x => x.BusId, "Vehicles", "VehicleId", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Schedules_Driver", x => x.DriverId, "Drivers", "DriverId", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex(
                name: "IX_Schedules_RouteBusDeparture",
                table: "Schedules",
                columns: new[] { "RouteId", "BusId", "DepartureTime" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedules");
        }
    }
}
