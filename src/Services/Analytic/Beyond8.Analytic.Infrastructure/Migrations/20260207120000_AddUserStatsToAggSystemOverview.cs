using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Analytic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatsToAggSystemOverview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalUsers",
                table: "AggSystemOverviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalActiveUsers",
                table: "AggSystemOverviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NewUsersToday",
                table: "AggSystemOverviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalInstructors",
                table: "AggSystemOverviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalStudents",
                table: "AggSystemOverviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "TotalUsers", table: "AggSystemOverviews");
            migrationBuilder.DropColumn(name: "TotalActiveUsers", table: "AggSystemOverviews");
            migrationBuilder.DropColumn(name: "NewUsersToday", table: "AggSystemOverviews");
            migrationBuilder.DropColumn(name: "TotalInstructors", table: "AggSystemOverviews");
            migrationBuilder.DropColumn(name: "TotalStudents", table: "AggSystemOverviews");
        }
    }
}
