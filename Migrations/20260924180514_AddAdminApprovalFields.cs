using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AdminApprovedAt",
                table: "Requisitions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdminApprovedByUserId",
                table: "Requisitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminRemarks",
                table: "Requisitions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_AdminApprovedByUserId",
                table: "Requisitions",
                column: "AdminApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Users_AdminApprovedByUserId",
                table: "Requisitions",
                column: "AdminApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Users_AdminApprovedByUserId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_AdminApprovedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "AdminApprovedAt",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "AdminApprovedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "AdminRemarks",
                table: "Requisitions");
        }
    }
}
