using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddRequisitionEditTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastEditReason",
                table: "Requisitions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "Requisitions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastEditedByUserId",
                table: "Requisitions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_LastEditedByUserId",
                table: "Requisitions",
                column: "LastEditedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Users_LastEditedByUserId",
                table: "Requisitions",
                column: "LastEditedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Users_LastEditedByUserId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_LastEditedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "LastEditReason",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "LastEditedByUserId",
                table: "Requisitions");
        }
    }
}
