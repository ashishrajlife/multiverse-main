using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPDemo.Migrations
{
    /// <inheritdoc />
    public partial class gpsdetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndAddress",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EndOdometerKm",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GpsDistanceKm",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GpsIdleMinutes",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GpsMaxSpeedKmph",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartAddress",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartOdometerKm",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GpsDevices",
                columns: table => new
                {
                    GpsDeviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Imei = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SimNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LastPingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpsDevices", x => x.GpsDeviceId);
                    table.ForeignKey(
                        name: "FK_GpsDevices_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleLocations",
                columns: table => new
                {
                    VehicleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId1 = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpeedKmph = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HeadingDegrees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ignition = table.Column<int>(type: "int", nullable: true),
                    OdometerKm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentTripId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleLocations", x => x.VehicleId);
                    table.ForeignKey(
                        name: "FK_VehicleLocations_Vehicles_VehicleId1",
                        column: x => x.VehicleId1,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GpsLogs",
                columns: table => new
                {
                    GpsLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GpsDeviceId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpeedKmph = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HeadingDegrees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ignition = table.Column<int>(type: "int", nullable: true),
                    OdometerKm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FuelLevelPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpsLogs", x => x.GpsLogId);
                    table.ForeignKey(
                        name: "FK_GpsLogs_GpsDevices_GpsDeviceId",
                        column: x => x.GpsDeviceId,
                        principalTable: "GpsDevices",
                        principalColumn: "GpsDeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GpsDevices_VehicleId",
                table: "GpsDevices",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_GpsLogs_GpsDeviceId",
                table: "GpsLogs",
                column: "GpsDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLocations_VehicleId1",
                table: "VehicleLocations",
                column: "VehicleId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GpsLogs");

            migrationBuilder.DropTable(
                name: "VehicleLocations");

            migrationBuilder.DropTable(
                name: "GpsDevices");

            migrationBuilder.DropColumn(
                name: "EndAddress",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "EndOdometerKm",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "GpsDistanceKm",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "GpsIdleMinutes",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "GpsMaxSpeedKmph",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "StartAddress",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "StartOdometerKm",
                table: "Trips");
        }
    }
}
