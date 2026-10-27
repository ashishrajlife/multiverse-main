using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPDemo.Migrations
{
    public partial class FixVehicleLocationIdentityAndPrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════
            // VehicleLocations — drop identity + recreate table
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                -- 1. Backup existing data
                IF OBJECT_ID(N'VehicleLocations_Bak', N'U') IS NOT NULL
                    DROP TABLE VehicleLocations_Bak;
                
                SELECT * INTO VehicleLocations_Bak FROM VehicleLocations;

                -- 2. Drop FK constraint
                DECLARE @fkName NVARCHAR(200);
                SELECT @fkName = name FROM sys.foreign_keys
                WHERE parent_object_id = OBJECT_ID(N'VehicleLocations');
                IF @fkName IS NOT NULL
                    EXEC('ALTER TABLE VehicleLocations DROP CONSTRAINT [' + @fkName + ']');

                -- 3. Drop PK constraint
                DECLARE @pkName NVARCHAR(200);
                SELECT @pkName = name FROM sys.key_constraints
                WHERE parent_object_id = OBJECT_ID(N'VehicleLocations') AND type = 'PK';
                IF @pkName IS NOT NULL
                    EXEC('ALTER TABLE VehicleLocations DROP CONSTRAINT [' + @pkName + ']');

                -- 4. Drop old table
                DROP TABLE VehicleLocations;

                -- 5. Recreate WITHOUT identity + with precision
                CREATE TABLE VehicleLocations (
                    VehicleId       INT NOT NULL PRIMARY KEY,
                    Latitude        DECIMAL(10,7) NOT NULL,
                    Longitude       DECIMAL(10,7) NOT NULL,
                    SpeedKmph       DECIMAL(6,2) NULL,
                    HeadingDegrees  DECIMAL(6,2) NULL,
                    Ignition        INT NULL,
                    OdometerKm      DECIMAL(12,2) NULL,
                    LastUpdatedAt   DATETIME2 NOT NULL,
                    Address         NVARCHAR(500) NULL,
                    CurrentTripId   INT NULL,
                    CONSTRAINT FK_VehicleLocations_Vehicles 
                        FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId)
                );

                -- 6. Restore data
                INSERT INTO VehicleLocations 
                SELECT VehicleId, Latitude, Longitude, SpeedKmph, HeadingDegrees, 
                       Ignition, OdometerKm, LastUpdatedAt, Address, CurrentTripId
                FROM VehicleLocations_Bak;

                -- 7. Cleanup
                DROP TABLE VehicleLocations_Bak;
            ");

            // ═══════════════════════════════════════════════════════
            // Other decimal precision fixes (table alter)
            // ═══════════════════════════════════════════════════════
            migrationBuilder.AlterColumn<decimal>(
                name: "SalaryAmount",
                table: "Users",
                type: "decimal(12,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityTons",
                table: "Vehicles",
                type: "decimal(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DistanceKm",
                table: "Trips",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Revenue",
                table: "Trips",
                type: "decimal(12,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FuelCost",
                table: "Trips",
                type: "decimal(12,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GpsDistanceKm",
                table: "Trips",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLocations_Vehicles_VehicleId",
                table: "VehicleLocations");

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityTons",
                table: "Vehicles",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SpeedKmph",
                table: "VehicleLocations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OdometerKm",
                table: "VehicleLocations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "VehicleLocations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "VehicleLocations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HeadingDegrees",
                table: "VehicleLocations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "VehicleLocations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "VehicleId1",
                table: "VehicleLocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "Revenue",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GpsDistanceKm",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FuelCost",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DistanceKm",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "MaintenanceLogs",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SpeedKmph",
                table: "GpsLogs",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OdometerKm",
                table: "GpsLogs",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "GpsLogs",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "GpsLogs",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HeadingDegrees",
                table: "GpsLogs",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FuelLevelPercent",
                table: "GpsLogs",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OdometerKm",
                table: "FuelLogs",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Liters",
                table: "FuelLogs",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "FuelLogs",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLocations_VehicleId1",
                table: "VehicleLocations",
                column: "VehicleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLocations_Vehicles_VehicleId1",
                table: "VehicleLocations",
                column: "VehicleId1",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
