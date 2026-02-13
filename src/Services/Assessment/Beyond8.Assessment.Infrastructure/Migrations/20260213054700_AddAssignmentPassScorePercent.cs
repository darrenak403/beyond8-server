using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Assessment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentPassScorePercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PassScorePercent",
                table: "Assignments",
                type: "integer",
                nullable: false,
                defaultValue: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassScorePercent",
                table: "Assignments");
        }
    }
}
