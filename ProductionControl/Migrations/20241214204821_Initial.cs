using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionControl.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentProductions",
                columns: table => new
                {
                    DepartmentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameDepartment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessRight = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentProductions", x => x.DepartmentID);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InnerException = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    User = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Machine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Application = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeExOrgs",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeExOrgAddInRegionID = table.Column<int>(type: "int", nullable: false),
                    NumberPass = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateEmployment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDismissal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDismissal = table.Column<bool>(type: "bit", nullable: false),
                    Descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DepartmentProductionDepartmentID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeExOrgs", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_EmployeeExOrgs_DepartmentProductions_DepartmentProductionDepartmentID",
                        column: x => x.DepartmentProductionDepartmentID,
                        principalTable: "DepartmentProductions",
                        principalColumn: "DepartmentID");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<long>(type: "bigint", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumGraf = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateEmployment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDismissal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDismissal = table.Column<bool>(type: "bit", nullable: false),
                    IsLunch = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Departments",
                        column: x => x.DepartmentID,
                        principalTable: "DepartmentProductions",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeExOrgAddInRegions",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    DepartmentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkingInTimeSheetEmployeeExOrg = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeExOrgAddInRegions", x => new { x.EmployeeID, x.DepartmentID });
                    table.ForeignKey(
                        name: "FK_EmployeeExOrgAddInRegions_EmployeeExOrgs_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "EmployeeExOrgs",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftDataExOrgs",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartmentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Hours = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftDataExOrgs", x => new { x.EmployeeID, x.WorkDate, x.DepartmentID });
                    table.ForeignKey(
                        name: "FK_ShiftDataExOrgs_EmployeeExOrgs_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "EmployeeExOrgs",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftsData",
                columns: table => new
                {
                    EmployeeID = table.Column<long>(type: "bigint", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hours = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Shift = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Overday = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHaveLunch = table.Column<bool>(type: "bit", nullable: false),
                    IsPreHoliday = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftsData", x => new { x.EmployeeID, x.WorkDate });
                    table.ForeignKey(
                        name: "FK_Employees",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentProductions_DepartmentID",
                table: "DepartmentProductions",
                column: "DepartmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeExOrgs_DepartmentProductionDepartmentID",
                table: "EmployeeExOrgs",
                column: "DepartmentProductionDepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentID",
                table: "Employees",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeID_DepartmentID_NumGraf",
                table: "Employees",
                columns: new[] { "EmployeeID", "DepartmentID", "NumGraf" },
                unique: true,
                filter: "[NumGraf] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftDataExOrgs_EmployeeID_WorkDate_DepartmentID",
                table: "ShiftDataExOrgs",
                columns: new[] { "EmployeeID", "WorkDate", "DepartmentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftsData_EmployeeID_WorkDate",
                table: "ShiftsData",
                columns: new[] { "EmployeeID", "WorkDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeExOrgAddInRegions");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "ShiftDataExOrgs");

            migrationBuilder.DropTable(
                name: "ShiftsData");

            migrationBuilder.DropTable(
                name: "EmployeeExOrgs");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "DepartmentProductions");
        }
    }
}
