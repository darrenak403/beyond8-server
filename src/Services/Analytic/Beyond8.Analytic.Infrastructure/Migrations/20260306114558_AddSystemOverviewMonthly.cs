using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Analytic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemOverviewMonthly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggSystemOverviewMonthlies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YearMonth = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    NewUsers = table.Column<int>(type: "integer", nullable: false),
                    NewStudents = table.Column<int>(type: "integer", nullable: false),
                    NewInstructors = table.Column<int>(type: "integer", nullable: false),
                    NewCourses = table.Column<int>(type: "integer", nullable: false),
                    NewPublishedCourses = table.Column<int>(type: "integer", nullable: false),
                    NewEnrollments = table.Column<int>(type: "integer", nullable: false),
                    NewCompletedEnrollments = table.Column<int>(type: "integer", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlatformProfit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    InstructorEarnings = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggSystemOverviewMonthlies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggSystemOverviewMonthlies_Year_Month",
                table: "AggSystemOverviewMonthlies",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_AggSystemOverviewMonthlies_YearMonth",
                table: "AggSystemOverviewMonthlies",
                column: "YearMonth",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggSystemOverviewMonthlies");
        }
    }
}
