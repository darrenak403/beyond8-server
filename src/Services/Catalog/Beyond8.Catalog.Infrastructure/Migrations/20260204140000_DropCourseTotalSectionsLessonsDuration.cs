using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropCourseTotalSectionsLessonsDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop columns if they still exist (e.g. RemoveCourseDenormalizedStats not applied or DB restored from backup).
            migrationBuilder.Sql(@"ALTER TABLE ""Courses"" DROP COLUMN IF EXISTS ""TotalSections"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Courses"" DROP COLUMN IF EXISTS ""TotalLessons"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Courses"" DROP COLUMN IF EXISTS ""TotalDurationMinutes"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalSections",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TotalLessons",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TotalDurationMinutes",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
