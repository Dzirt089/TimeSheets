using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeSheets.Migrations
{
    /// <inheritdoc />
    public partial class NewTableForPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Employee");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeePhotos");

            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "Employee",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
