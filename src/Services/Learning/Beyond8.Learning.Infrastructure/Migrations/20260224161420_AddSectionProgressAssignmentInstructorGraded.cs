using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Learning.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionProgressAssignmentInstructorGraded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AssignmentInstructorGraded",
                table: "SectionProgresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentInstructorGraded",
                table: "SectionProgresses");
        }
    }
}
