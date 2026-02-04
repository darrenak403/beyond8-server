using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTotalStudentsBack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalStudents",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalStudents",
                table: "Courses");
        }
    }
}
