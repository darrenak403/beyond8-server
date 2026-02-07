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
            migrationBuilder.Sql(@"
                ALTER TABLE ""AggSystemOverviews""
                ADD COLUMN IF NOT EXISTS ""TotalUsers"" integer NOT NULL DEFAULT 0,
                ADD COLUMN IF NOT EXISTS ""TotalActiveUsers"" integer NOT NULL DEFAULT 0,
                ADD COLUMN IF NOT EXISTS ""NewUsersToday"" integer NOT NULL DEFAULT 0,
                ADD COLUMN IF NOT EXISTS ""TotalInstructors"" integer NOT NULL DEFAULT 0,
                ADD COLUMN IF NOT EXISTS ""TotalStudents"" integer NOT NULL DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""AggSystemOverviews""
                DROP COLUMN IF EXISTS ""TotalUsers"",
                DROP COLUMN IF EXISTS ""TotalActiveUsers"",
                DROP COLUMN IF EXISTS ""NewUsersToday"",
                DROP COLUMN IF EXISTS ""TotalInstructors"",
                DROP COLUMN IF EXISTS ""TotalStudents"";
            ");
        }
    }
}
