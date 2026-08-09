using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TmsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddYearToEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Enrollments",
                type: "integer",
                nullable: false,
                defaultValue: 2026);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "Enrollments");
        }
    }
}
