using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERPDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddRequisition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Organizations_OrganizationId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Departments_OrganizationId",
                table: "Departments");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Departments");

            migrationBuilder.CreateTable(
                name: "Requisitions",
                columns: table => new
                {
                    RequisitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequisitionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequiredByDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalEstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HodApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    HodApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HodRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisitions", x => x.RequisitionId);
                    table.ForeignKey(
                        name: "FK_Requisitions_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_Requisitions_Users_HodApprovedByUserId",
                        column: x => x.HodApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Requisitions_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequisitionItems",
                columns: table => new
                {
                    RequisitionItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequisitionId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstimatedRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisitionItems", x => x.RequisitionItemId);
                    table.ForeignKey(
                        name: "FK_RequisitionItems_Requisitions_RequisitionId",
                        column: x => x.RequisitionId,
                        principalTable: "Requisitions",
                        principalColumn: "RequisitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequisitionItems_RequisitionId",
                table: "RequisitionItems",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_DepartmentId",
                table: "Requisitions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_HodApprovedByUserId",
                table: "Requisitions",
                column: "HodApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_RequestedByUserId",
                table: "Requisitions",
                column: "RequestedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequisitionItems");

            migrationBuilder.DropTable(
                name: "Requisitions");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Departments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    GSTIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    GSTLastVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GSTStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Plan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TradeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationId);
                });

            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "OrganizationId", "Address", "City", "Code", "Country", "CreatedAt", "Email", "GSTIN", "GSTLastVerifiedAt", "GSTStatus", "IsActive", "LegalName", "Name", "Phone", "Plan", "TradeName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "Mumbai", "MV-HQ", "India", new DateTime(2026, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), "hq@multiverse.io", null, null, null, true, null, "Multiverse HQ", "9999999990", "Enterprise", null, null },
                    { 2, null, "Bengaluru", "ACME-001", "India", new DateTime(2026, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), "contact@acme.com", null, null, null, true, null, "Acme Pvt Ltd", "9999999911", "Pro", null, null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "CreatedAt", "Description", "RoleName" },
                values: new object[] { 4, new DateTime(2026, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), "Full system access - super administrator", "SuperAdmin" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "OrganizationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "OrganizationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "OrganizationId",
                value: 2);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "AadhaarNumber", "Address", "City", "CreatedAt", "DepartmentId", "Designation", "Email", "EmploymentType", "FullName", "IsActive", "JoiningDate", "LastLoginAt", "OrganizationId", "PanNumber", "Password", "PhoneNumber", "Pincode", "PlantLocation", "RoleId", "SalaryAmount", "SalaryType", "ShiftEndTime", "ShiftStartTime", "State", "UpdatedAt", "Username" },
                values: new object[] { 1, null, null, null, new DateTime(2026, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), null, null, "superadmin@erp.com", null, "Super Admin", true, null, null, 1, null, "super123", "9999999991", null, null, 4, null, null, null, null, null, null, "superadmin" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OrganizationId",
                table: "Departments",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Organizations_OrganizationId",
                table: "Departments",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
