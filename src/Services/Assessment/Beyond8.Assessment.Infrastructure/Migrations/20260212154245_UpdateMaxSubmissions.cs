using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Assessment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaxSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rubric",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "TotalSubmissions",
                table: "Assignments",
                newName: "MaxSubmissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxSubmissions",
                table: "Assignments",
                newName: "TotalSubmissions");

            migrationBuilder.AddColumn<string>(
                name: "Rubric",
                table: "Assignments",
                type: "jsonb",
                nullable: true);
        }
    }
}
