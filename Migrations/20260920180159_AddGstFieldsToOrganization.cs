using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddGstFieldsToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GSTIN",
                table: "Organizations",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GSTLastVerifiedAt",
                table: "Organizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GSTStatus",
                table: "Organizations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "Organizations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeName",
                table: "Organizations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Organizations",
                keyColumn: "OrganizationId",
                keyValue: 1,
                columns: new[] { "GSTIN", "GSTLastVerifiedAt", "GSTStatus", "LegalName", "TradeName" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Organizations",
                keyColumn: "OrganizationId",
                keyValue: 2,
                columns: new[] { "GSTIN", "GSTLastVerifiedAt", "GSTStatus", "LegalName", "TradeName" },
                values: new object[] { null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GSTIN",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GSTLastVerifiedAt",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GSTStatus",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "TradeName",
                table: "Organizations");
        }
    }
}
