using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionControl.DataAccess.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class NewCardNumberForEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "EmployeeExOrgs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "EmployeeExOrgs");
        }
    }
}
