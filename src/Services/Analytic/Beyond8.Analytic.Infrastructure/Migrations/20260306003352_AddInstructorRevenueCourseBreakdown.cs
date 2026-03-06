using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Analytic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorRevenueCourseBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PendingBalance",
                table: "AggInstructorRevenues",
                newName: "AvailableBalance");

            migrationBuilder.AddColumn<int>(
                name: "PublishedCourses",
                table: "AggInstructorRevenues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RejectedCourses",
                table: "AggInstructorRevenues",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedCourses",
                table: "AggInstructorRevenues");

            migrationBuilder.DropColumn(
                name: "RejectedCourses",
                table: "AggInstructorRevenues");

            migrationBuilder.RenameColumn(
                name: "AvailableBalance",
                table: "AggInstructorRevenues",
                newName: "PendingBalance");
        }
    }
}
