using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Analytic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemOverviewDaily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggSystemOverviewDailies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateKey = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AggSystemOverviewDailies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggSystemOverviewDailies_DateKey",
                table: "AggSystemOverviewDailies",
                column: "DateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AggSystemOverviewDailies_Year_Month_Day",
                table: "AggSystemOverviewDailies",
                columns: new[] { "Year", "Month", "Day" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggSystemOverviewDailies");
        }
    }
}
