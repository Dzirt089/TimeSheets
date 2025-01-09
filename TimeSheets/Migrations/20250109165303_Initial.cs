using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TimeSheets.Migrations
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
                name: "Employee",
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
                    IsLunch = table.Column<bool>(type: "bit", nullable: false),
                    NumberPass = table.Column<int>(type: "int", nullable: true),
                    Descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Departments",
                        column: x => x.DepartmentID,
                        principalTable: "DepartmentProductions",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePhotos",
                columns: table => new
                {
                    EmployeeID = table.Column<long>(type: "bigint", nullable: false),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePhotos", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_EmployeePhotos_Employee_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employee",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
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
                        principalTable: "Employee",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DepartmentProductions",
                columns: new[] { "DepartmentID", "AccessRight", "NameDepartment" },
                values: new object[,]
                {
                    { "01", 0, "Группа разработки" },
                    { "02", 0, "Группа аналитики" },
                    { "03", 0, "Группа тестирования" },
                    { "04", 0, "Группа исследования" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentProductions_DepartmentID",
                table: "DepartmentProductions",
                column: "DepartmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DepartmentID",
                table: "Employee",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_EmployeeID_DepartmentID_NumGraf",
                table: "Employee",
                columns: new[] { "EmployeeID", "DepartmentID", "NumGraf" },
                unique: true,
                filter: "[NumGraf] IS NOT NULL");

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
                name: "EmployeePhotos");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "ShiftsData");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "DepartmentProductions");
        }
    }
}
