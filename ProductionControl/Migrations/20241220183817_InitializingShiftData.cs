using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductionControl.Migrations
{
    /// <inheritdoc />
    public partial class InitializingShiftData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DepartmentProductions",
                keyColumn: "DepartmentID",
                keyValue: "01");

            migrationBuilder.DeleteData(
                table: "DepartmentProductions",
                keyColumn: "DepartmentID",
                keyValue: "02");

            migrationBuilder.DeleteData(
                table: "DepartmentProductions",
                keyColumn: "DepartmentID",
                keyValue: "03");

            migrationBuilder.DeleteData(
                table: "DepartmentProductions",
                keyColumn: "DepartmentID",
                keyValue: "04");
        }
    }
}
