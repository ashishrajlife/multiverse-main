using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddPartyAndRealTripFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceAmount",
                table: "Trips",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightAmount",
                table: "Trips",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeight",
                table: "Trips",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LrNumber",
                table: "Trips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialName",
                table: "Trips",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartyId",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TareWeight",
                table: "Trips",
                type: "decimal(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "Trips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    PartyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Gstin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.PartyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_PartyId",
                table: "Trips",
                column: "PartyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Parties_PartyId",
                table: "Trips",
                column: "PartyId",
                principalTable: "Parties",
                principalColumn: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Parties_PartyId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.DropIndex(
                name: "IX_Trips_PartyId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "AdvanceAmount",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "FreightAmount",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "GrossWeight",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "LrNumber",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "MaterialName",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PartyId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TareWeight",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Trips");
        }
    }
}
