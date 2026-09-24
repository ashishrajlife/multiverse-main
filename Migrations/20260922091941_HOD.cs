using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPDemo.Migrations
{
    /// <inheritdoc />
    public partial class HOD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "CreatedAt", "Description", "RoleName" },
                values: new object[] { 5, new DateTime(2026, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), "Full system access - specific department", "HOD" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 5);
        }
    }
}
