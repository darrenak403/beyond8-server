using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Analytic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseStatusForInstructorRevenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedCourses",
                table: "AggInstructorRevenues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ArchivedCourses",
                table: "AggInstructorRevenues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DraftCourses",
                table: "AggInstructorRevenues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PendingApprovalCourses",
                table: "AggInstructorRevenues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SuspendedCourses",
                table: "AggInstructorRevenues",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedCourses",
                table: "AggInstructorRevenues");

            migrationBuilder.DropColumn(
                name: "ArchivedCourses",
                table: "AggInstructorRevenues");

            migrationBuilder.DropColumn(
                name: "DraftCourses",
                table: "AggInstructorRevenues");

            migrationBuilder.DropColumn(
                name: "PendingApprovalCourses",
                table: "AggInstructorRevenues");

            migrationBuilder.DropColumn(
                name: "SuspendedCourses",
                table: "AggInstructorRevenues");
        }
    }
}
